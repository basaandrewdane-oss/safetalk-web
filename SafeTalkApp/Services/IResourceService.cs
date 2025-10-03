using SafeTalkApp.DTOs.Resources;
using SafeTalkApp.DTOs.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{
    public interface IResourceService
    {
        ApiResponse<IEnumerable<ResourcesDTO>> GetResources();
        ApiResponse<ResourcesDTO> AddResource(ResourcesDTO model);
        ApiResponse<ResourcesDTO> EditResource(ResourcesDTO model);
        ApiResponse<bool> DeleteResource(int id);
    }
}