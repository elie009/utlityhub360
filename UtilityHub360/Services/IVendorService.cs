using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface IVendorService
    {
        Task<ApiResponse<VendorDto>> CreateVendorAsync(CreateVendorDto createVendorDto, string userId);
        Task<ApiResponse<VendorDto>> GetVendorAsync(string vendorId, string userId);
        Task<ApiResponse<List<VendorDto>>> GetVendorsAsync(string userId, bool? isActive = null);
        Task<ApiResponse<VendorDto>> UpdateVendorAsync(string vendorId, UpdateVendorDto updateVendorDto, string userId);
        Task<ApiResponse<bool>> DeleteVendorAsync(string vendorId, string userId);
    }
}

