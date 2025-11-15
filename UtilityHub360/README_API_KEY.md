# OpenAI API Key Configuration

## Setup Instructions

The OpenAI API key is now read from configuration instead of being hardcoded. 

### For Local Development:

1. **Option 1: Update appsettings.Development.json**
   - Open `appsettings.Development.json`
   - Replace `YOUR_OPENAI_API_KEY_HERE` with your actual API key

2. **Option 2: Use User Secrets (Recommended)**
   ```bash
   dotnet user-secrets set "OpenAISettings:ApiKey" "your-actual-api-key-here"
   ```

3. **Option 3: Use Environment Variables**
   ```bash
   # Windows PowerShell
   $env:OpenAISettings__ApiKey = "your-actual-api-key-here"
   
   # Linux/Mac
   export OpenAISettings__ApiKey="your-actual-api-key-here"
   ```

### For Production:

**Always use environment variables or Azure Key Vault - never commit real API keys!**

1. **Azure App Service:**
   - Go to Configuration → Application Settings
   - Add: `OpenAISettings__ApiKey` = `your-actual-api-key`

2. **Environment Variables:**
   ```bash
   OpenAISettings__ApiKey=your-actual-api-key-here
   ```

3. **Docker:**
   ```yaml
   environment:
     - OpenAISettings__ApiKey=${OPENAI_API_KEY}
   ```

## Security Notes

- ✅ API keys are now read from configuration (appsettings.json or environment variables)
- ✅ No hardcoded API keys in source code
- ✅ appsettings files use placeholders (`YOUR_OPENAI_API_KEY_HERE`)
- ⚠️ Never commit real API keys to git
- ⚠️ Use environment variables or secure key storage for production

## Configuration Priority

The application reads the API key in this order:
1. Environment variables (`OpenAISettings__ApiKey`)
2. User Secrets (development only)
3. appsettings.Development.json (development only)
4. appsettings.json (fallback)

