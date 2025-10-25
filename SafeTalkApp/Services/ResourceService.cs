using SafeTalkApp.DTOs.Resources;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{
    public class ResourceService : IResourceService
    {
        private readonly ISafeTalkAppContext _db;

        public ResourceService(ISafeTalkAppContext db)
        {
            _db = db;
        }

        public ApiResponse<IEnumerable<ResourcesDTO>> GetResources()
        {
            try
            {
                var resources = _db.resource_tbl.Select(r => new ResourcesDTO
                {
                    resourceID = r.resourceID,
                    title = r.title,
                    content = r.content,
                    category = r.category,
                    type = r.type,
                    url = r.url,
                    source = r.source,
                    publishedDate = r.publishedDate,
                    dateCreated = r.dateCreated,
                    dateUpdated = r.dateUpdated
                }).ToList();

                return ApiResponse<IEnumerable<ResourcesDTO>>.Ok(resources);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<ResourcesDTO>>.Fail("Error getting resources: " + ex.Message);
            }
        }

        public ApiResponse<ResourcesDTO> AddResource(ResourcesDTO model)
        {
            try
            {
                var entity = new ResourceTblModel
                {
                    title = model.title,
                    content = model.content,
                    category = model.category,
                    type = model.type,
                    url = model.url,
                    source = model.source,
                    publishedDate = model.publishedDate,
                    dateCreated = DateTime.Now,
                    dateUpdated = DateTime.Now
                };
                _db.resource_tbl.Add(entity);
                _db.SaveChanges();
                model.resourceID = entity.resourceID; // Set the generated ID back to the DTO
                return ApiResponse<ResourcesDTO>.Ok(model);
            }
            catch (Exception ex)
            {
                return ApiResponse<ResourcesDTO>.Fail("Error adding resource: " + ex.Message);
            }
        }

        public ApiResponse<ResourcesDTO> EditResource(ResourcesDTO model)
        {
            try
            {
                var existing = _db.resource_tbl.Find(model.resourceID);
                if (existing != null)
                {
                    existing.title = model.title;
                    existing.content = model.content;
                    existing.category = model.category;
                    existing.type = model.type;
                    existing.url = model.url;
                    existing.publishedDate = model.publishedDate;
                    existing.dateUpdated = DateTime.Now;
                    _db.SaveChanges();
                    return ApiResponse<ResourcesDTO>.Ok(model);
                }
                return ApiResponse<ResourcesDTO>.Fail("Resource not found");
            }
            catch (Exception ex)
            {
                return ApiResponse<ResourcesDTO>.Fail("Error editing resource: " + ex.Message);
            }
        }

        public ApiResponse<bool> DeleteResource(int id)
        {
            try
            {
                var existing = _db.resource_tbl.Find(id);
                if (existing != null)
                {
                    _db.resource_tbl.Remove(existing);
                    _db.SaveChanges();
                    return ApiResponse<bool>.Ok(true);
                }
                return ApiResponse<bool>.Fail("Resource not found");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail("Error deleting resource: " + ex.Message);
            }
        }
    }
}