using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using UrbaPF.Application.Interfaces;
using UrbaPF.Application.DTOs;

namespace UrbaPF.Application.Interfaces;

public interface IUserService
{
    Task<Guid?> UploadUserPhotoAsync(Guid userId, IFormFile file, ClaimsPrincipal user);
}