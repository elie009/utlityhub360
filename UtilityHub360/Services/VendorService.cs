using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class VendorService : IVendorService
    {
        private readonly ApplicationDbContext _context;

        public VendorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<VendorDto>> CreateVendorAsync(CreateVendorDto createVendorDto, string userId)
        {
            try
            {
                var vendor = new Vendor
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Name = createVendorDto.Name,
                    ContactPerson = createVendorDto.ContactPerson,
                    Email = createVendorDto.Email,
                    Phone = createVendorDto.Phone,
                    Address = createVendorDto.Address,
                    Website = createVendorDto.Website,
                    Category = createVendorDto.Category,
                    AccountNumber = createVendorDto.AccountNumber,
                    Notes = createVendorDto.Notes,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Vendors.Add(vendor);
                await _context.SaveChangesAsync();

                var vendorDto = await MapToVendorDtoAsync(vendor);
                return ApiResponse<VendorDto>.SuccessResult(vendorDto, "Vendor created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<VendorDto>.ErrorResult($"Failed to create vendor: {ex.Message}");
            }
        }

        public async Task<ApiResponse<VendorDto>> GetVendorAsync(string vendorId, string userId)
        {
            try
            {
                var vendor = await _context.Vendors
                    .FirstOrDefaultAsync(v => v.Id == vendorId && v.UserId == userId);

                if (vendor == null)
                {
                    return ApiResponse<VendorDto>.ErrorResult("Vendor not found");
                }

                var vendorDto = await MapToVendorDtoAsync(vendor);
                return ApiResponse<VendorDto>.SuccessResult(vendorDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<VendorDto>.ErrorResult($"Failed to get vendor: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<VendorDto>>> GetVendorsAsync(string userId, bool? isActive = null)
        {
            try
            {
                var query = _context.Vendors
                    .Where(v => v.UserId == userId);

                if (isActive.HasValue)
                {
                    query = query.Where(v => v.IsActive == isActive.Value);
                }

                var vendors = await query
                    .OrderBy(v => v.Name)
                    .ToListAsync();

                var vendorDtos = new List<VendorDto>();
                foreach (var vendor in vendors)
                {
                    vendorDtos.Add(await MapToVendorDtoAsync(vendor));
                }

                return ApiResponse<List<VendorDto>>.SuccessResult(vendorDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<VendorDto>>.ErrorResult($"Failed to get vendors: {ex.Message}");
            }
        }

        public async Task<ApiResponse<VendorDto>> UpdateVendorAsync(string vendorId, UpdateVendorDto updateVendorDto, string userId)
        {
            try
            {
                var vendor = await _context.Vendors
                    .FirstOrDefaultAsync(v => v.Id == vendorId && v.UserId == userId);

                if (vendor == null)
                {
                    return ApiResponse<VendorDto>.ErrorResult("Vendor not found");
                }

                if (!string.IsNullOrEmpty(updateVendorDto.Name))
                    vendor.Name = updateVendorDto.Name;

                if (updateVendorDto.ContactPerson != null)
                    vendor.ContactPerson = updateVendorDto.ContactPerson;

                if (updateVendorDto.Email != null)
                    vendor.Email = updateVendorDto.Email;

                if (updateVendorDto.Phone != null)
                    vendor.Phone = updateVendorDto.Phone;

                if (updateVendorDto.Address != null)
                    vendor.Address = updateVendorDto.Address;

                if (updateVendorDto.Website != null)
                    vendor.Website = updateVendorDto.Website;

                if (updateVendorDto.Category != null)
                    vendor.Category = updateVendorDto.Category;

                if (updateVendorDto.AccountNumber != null)
                    vendor.AccountNumber = updateVendorDto.AccountNumber;

                if (updateVendorDto.Notes != null)
                    vendor.Notes = updateVendorDto.Notes;

                if (updateVendorDto.IsActive.HasValue)
                    vendor.IsActive = updateVendorDto.IsActive.Value;

                vendor.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var vendorDto = await MapToVendorDtoAsync(vendor);
                return ApiResponse<VendorDto>.SuccessResult(vendorDto, "Vendor updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<VendorDto>.ErrorResult($"Failed to update vendor: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteVendorAsync(string vendorId, string userId)
        {
            try
            {
                var vendor = await _context.Vendors
                    .FirstOrDefaultAsync(v => v.Id == vendorId && v.UserId == userId);

                if (vendor == null)
                {
                    return ApiResponse<bool>.ErrorResult("Vendor not found");
                }

                // Check if vendor has bills
                var hasBills = await _context.Bills
                    .AnyAsync(b => b.Provider == vendor.Name && b.UserId == userId && !b.IsDeleted);

                if (hasBills)
                {
                    // Soft delete - just deactivate
                    vendor.IsActive = false;
                    vendor.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return ApiResponse<bool>.SuccessResult(true, "Vendor deactivated (has associated bills)");
                }

                // Hard delete if no bills
                _context.Vendors.Remove(vendor);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Vendor deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete vendor: {ex.Message}");
            }
        }

        private async Task<VendorDto> MapToVendorDtoAsync(Vendor vendor)
        {
            // Get bill statistics for this vendor
            var bills = await _context.Bills
                .Where(b => b.Provider == vendor.Name && 
                           b.UserId == vendor.UserId && 
                           !b.IsDeleted)
                .ToListAsync();

            var paidBills = bills.Where(b => b.Status == "PAID").ToList();

            return new VendorDto
            {
                Id = vendor.Id,
                UserId = vendor.UserId,
                Name = vendor.Name,
                ContactPerson = vendor.ContactPerson,
                Email = vendor.Email,
                Phone = vendor.Phone,
                Address = vendor.Address,
                Website = vendor.Website,
                Category = vendor.Category,
                AccountNumber = vendor.AccountNumber,
                Notes = vendor.Notes,
                IsActive = vendor.IsActive,
                CreatedAt = vendor.CreatedAt,
                UpdatedAt = vendor.UpdatedAt,
                BillCount = bills.Count,
                TotalPaid = paidBills.Sum(b => b.Amount)
            };
        }
    }
}

