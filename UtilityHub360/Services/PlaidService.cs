using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    /// <summary>
    /// Service for interacting with Plaid API via REST
    /// </summary>
    public class PlaidService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PlaidService> _logger;
        private readonly PlaidSettings _settings;
        private readonly string _baseUrl;

        public PlaidService(PlaidSettings settings, ILogger<PlaidService> logger, IHttpClientFactory httpClientFactory)
        {
            _settings = settings;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _baseUrl = GetEnvironmentUrl(_settings.Environment);
        }

        /// <summary>
        /// Create a Link token for Plaid Link initialization
        /// </summary>
        public async Task<string> CreateLinkTokenAsync(string userId, string? webhookUrl = null)
        {
            try
            {
                var request = new
                {
                    client_id = _settings.ClientId,
                    secret = _settings.Secret,
                    client_name = "UtilityHub360",
                    products = new[] { "transactions" },
                    country_codes = new[] { "US", "CA" },
                    language = "en",
                    user = new
                    {
                        client_user_id = userId
                    },
                    webhook = webhookUrl
                };

                var response = await PostAsync<LinkTokenResponse>("/link/token/create", request);
                return response.LinkToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Plaid Link token for user {UserId}", userId);
                throw new Exception($"Failed to create Plaid Link token: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Exchange public token for access token
        /// </summary>
        public async Task<ItemPublicTokenExchangeResponse> ExchangePublicTokenAsync(string publicToken)
        {
            try
            {
                var request = new
                {
                    client_id = _settings.ClientId,
                    secret = _settings.Secret,
                    public_token = publicToken
                };

                var response = await PostAsync<ItemPublicTokenExchangeResponse>("/item/public_token/exchange", request);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exchanging Plaid public token");
                throw new Exception($"Failed to exchange public token: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get account information from Plaid
        /// </summary>
        public async Task<AccountsGetResponse> GetAccountsAsync(string accessToken)
        {
            try
            {
                var request = new
                {
                    client_id = _settings.ClientId,
                    secret = _settings.Secret,
                    access_token = accessToken
                };

                var response = await PostAsync<AccountsGetResponse>("/accounts/get", request);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching accounts from Plaid");
                throw new Exception($"Failed to fetch accounts: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get transactions from Plaid
        /// </summary>
        public async Task<TransactionsGetResponse> GetTransactionsAsync(
            string accessToken,
            DateTime startDate,
            DateTime endDate,
            List<string>? accountIds = null)
        {
            try
            {
                var request = new
                {
                    client_id = _settings.ClientId,
                    secret = _settings.Secret,
                    access_token = accessToken,
                    start_date = startDate.ToString("yyyy-MM-dd"),
                    end_date = endDate.ToString("yyyy-MM-dd"),
                    account_ids = accountIds
                };

                var response = await PostAsync<TransactionsGetResponse>("/transactions/get", request);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transactions from Plaid");
                throw new Exception($"Failed to fetch transactions: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Remove an item (disconnect account)
        /// </summary>
        public async Task<ItemRemoveResponse> RemoveItemAsync(string accessToken)
        {
            try
            {
                var request = new
                {
                    client_id = _settings.ClientId,
                    secret = _settings.Secret,
                    access_token = accessToken
                };

                var response = await PostAsync<ItemRemoveResponse>("/item/remove", request);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing Plaid item");
                throw new Exception($"Failed to remove item: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Make a POST request to Plaid API
        /// </summary>
        private async Task<T> PostAsync<T>(string endpoint, object request)
        {
            var url = $"{_baseUrl}{endpoint}";
            
            // Plaid API uses snake_case
            // For requests, we use anonymous objects with snake_case property names
            // For responses, we use JsonPropertyName attributes
            var requestOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false
            };
            
            var responseOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false, // Use JsonPropertyName attributes
                WriteIndented = false,
                Converters = { new DateOnlyJsonConverter() }
            };
            
            var json = JsonSerializer.Serialize(request, requestOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpResponse = await _httpClient.PostAsync(url, content);
            var responseContent = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
            {
                var error = JsonSerializer.Deserialize<PlaidErrorResponse>(responseContent, responseOptions);
                throw new Exception($"Plaid API error: {error?.ErrorMessage ?? responseContent}");
            }

            var result = JsonSerializer.Deserialize<T>(responseContent, responseOptions);

            if (result == null)
            {
                throw new Exception("Failed to deserialize Plaid API response");
            }

            return result;
        }

        /// <summary>
        /// Get the Plaid environment URL
        /// </summary>
        private string GetEnvironmentUrl(string environment)
        {
            return environment.ToLower() switch
            {
                "production" => "https://production.plaid.com",
                "development" => "https://development.plaid.com",
                _ => "https://sandbox.plaid.com" // sandbox is default
            };
        }
    }

    // Response DTOs with JsonPropertyName attributes for snake_case mapping
    public class LinkTokenResponse
    {
        [JsonPropertyName("link_token")]
        public string LinkToken { get; set; } = string.Empty;
        
        [JsonPropertyName("expiration")]
        public DateTime Expiration { get; set; }
    }

    public class ItemPublicTokenExchangeResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
        
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; } = string.Empty;
    }

    public class AccountsGetResponse
    {
        [JsonPropertyName("accounts")]
        public List<PlaidAccount> Accounts { get; set; } = new();
        
        [JsonPropertyName("item")]
        public PlaidItem Item { get; set; } = new();
    }

    public class PlaidAccount
    {
        [JsonPropertyName("account_id")]
        public string AccountId { get; set; } = string.Empty;
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("mask")]
        public string? Mask { get; set; }
        
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonPropertyName("subtype")]
        public string Subtype { get; set; } = string.Empty;
        
        [JsonPropertyName("balances")]
        public PlaidAccountBalance? Balances { get; set; }
    }

    public class PlaidAccountBalance
    {
        [JsonPropertyName("available")]
        public decimal? Available { get; set; }
        
        [JsonPropertyName("current")]
        public decimal? Current { get; set; }
        
        [JsonPropertyName("limit")]
        public decimal? Limit { get; set; }
    }

    public class PlaidItem
    {
        [JsonPropertyName("item_id")]
        public string ItemId { get; set; } = string.Empty;
        
        [JsonPropertyName("institution_id")]
        public string InstitutionId { get; set; } = string.Empty;
    }

    public class TransactionsGetResponse
    {
        [JsonPropertyName("transactions")]
        public List<PlaidTransaction> Transactions { get; set; } = new();
        
        [JsonPropertyName("total_transactions")]
        public int TotalTransactions { get; set; }
        
        [JsonPropertyName("request_id")]
        public string? RequestId { get; set; }
    }

    public class PlaidTransaction
    {
        [JsonPropertyName("transaction_id")]
        public string TransactionId { get; set; } = string.Empty;
        
        [JsonPropertyName("account_id")]
        public string AccountId { get; set; } = string.Empty;
        
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("merchant_name")]
        public string? MerchantName { get; set; }
        
        [JsonPropertyName("amount")]
        public decimal? Amount { get; set; }
        
        [JsonPropertyName("date")]
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly Date { get; set; }
        
        [JsonPropertyName("category")]
        public List<string>? Category { get; set; }
        
        [JsonPropertyName("reference_number")]
        public string? ReferenceNumber { get; set; }
        
        [JsonPropertyName("location")]
        public PlaidTransactionLocation? Location { get; set; }
        
        [JsonPropertyName("pending")]
        public bool Pending { get; set; }
        
        [JsonPropertyName("payment_channel")]
        public string? PaymentChannel { get; set; }
    }

    public class PlaidTransactionLocation
    {
        [JsonPropertyName("city")]
        public string? City { get; set; }
        
        [JsonPropertyName("region")]
        public string? Region { get; set; }
        
        [JsonPropertyName("country")]
        public string? Country { get; set; }
    }

    public class ItemRemoveResponse
    {
        [JsonPropertyName("removed")]
        public bool Removed { get; set; }
        
        [JsonPropertyName("request_id")]
        public string? RequestId { get; set; }
    }

    public class PlaidErrorResponse
    {
        [JsonPropertyName("error_type")]
        public string? ErrorType { get; set; }
        
        [JsonPropertyName("error_code")]
        public string? ErrorCode { get; set; }
        
        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }
        
        [JsonPropertyName("display_message")]
        public string? DisplayMessage { get; set; }
    }

    // Custom converter for DateOnly (Plaid returns dates as strings in YYYY-MM-DD format)
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrEmpty(value))
                return DateOnly.MinValue;
            
            return DateOnly.Parse(value);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
        }
    }
}
