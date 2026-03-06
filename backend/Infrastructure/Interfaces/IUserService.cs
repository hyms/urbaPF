using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using UrbaPF.Infrastructure.Interfaces;
using UrbaPF.Infrastructure.DTOs;

namespace UrbaPF.Infrastructure.Interfaces;

public interface IUserService
{
    Task<Guid?> UploadUserPhotoAsync(Guid userId, IFormFile file, ClaimsPrincipal user);
}