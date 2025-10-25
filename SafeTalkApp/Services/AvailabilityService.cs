using SafeTalkApp.DTOs.Account;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly ISafeTalkAppContext _db;

        public AvailabilityService(ISafeTalkAppContext db)
        {
            _db = db;
        }

        public ApiResponse<IEnumerable<AvailabilityDTO>> GetAvailability(int userID)
        {
            try
            {
                var availabilities = (from a in _db.user_availability_tbl
                                      join d in _db.days_of_week_tbl on a.dayID equals d.dayID
                                      join u in _db.user_tbl on a.userID equals u.userID
                                      where a.userID == userID
                                      select new
                                      {
                                          a.availabilityID,
                                          d.dayID,
                                          d.day,
                                          a.availabilityStart,
                                          a.availabilityEnd,
                                          u.slotDuration,
                                          a.fee
                                      })
                                      .ToList() // <-- materialize here
                                      .Select(a => new AvailabilityDTO
                                      {
                                          availabilityID = a.availabilityID,
                                          dayID = a.dayID,
                                          day = a.day,
                                          slotDuration = a.slotDuration,
                                          startTime = a.availabilityStart,
                                          endTime = a.availabilityEnd,
                                          availabilityStart = a.availabilityStart.ToString(@"hh\:mm"),
                                          availabilityEnd = a.availabilityEnd.ToString(@"hh\:mm"),
                                          fee = a.fee
                                      })
                                      .ToList();

                return ApiResponse<IEnumerable<AvailabilityDTO>>.Ok(availabilities);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<AvailabilityDTO>>.Fail("An error occurred while retrieving availability." + ex.Message);
            }
        }

        public ApiResponse<bool> SaveAvailability(int userID, List<AvailabilityDTO> availabilities)
        {
            try
            {
                var now = DateTime.Now;

                // Get user record (since slotDuration is stored in user_tbl)
                var user = _db.user_tbl.FirstOrDefault(u => u.userID == userID);
                if (user == null)
                    return ApiResponse<bool>.Fail("User not found.");

                // Get existing availability for this user
                var existingList = _db.user_availability_tbl
                    .Where(a => a.userID == userID)
                    .ToList();

                // --- Handle slot duration update ---
                // (Take slotDuration from the first record if multiple provided)
                if (availabilities != null && availabilities.Any())
                {
                    var newSlotDuration = availabilities.FirstOrDefault()?.slotDuration ?? user.slotDuration;

                    // Only update if changed
                    if (newSlotDuration > 0 && newSlotDuration != user.slotDuration)
                    {
                        user.slotDuration = newSlotDuration;
                        user.dateUpdated = now;
                    }
                }

                // --- Handle availabilities ---
                foreach (var dto in availabilities)
                {
                    var existing = existingList.FirstOrDefault(x => x.dayID == dto.dayID);

                    if (existing != null)
                    {
                        // Update existing record
                        existing.availabilityStart = TimeSpan.Parse(dto.availabilityStart);
                        existing.availabilityEnd = TimeSpan.Parse(dto.availabilityEnd);
                        existing.fee = dto.fee > 0 ? dto.fee : existing.fee;
                        existing.dateUpdated = now;
                    }
                    else
                    {
                        // Insert new record
                        var newItem = new UserAvailabilityTblModel
                        {
                            userID = userID,
                            dayID = dto.dayID,
                            availabilityStart = TimeSpan.Parse(dto.availabilityStart),
                            availabilityEnd = TimeSpan.Parse(dto.availabilityEnd),
                            fee = dto.fee > 0 ? dto.fee : 500, // default fee
                            dateCreated = now,
                            dateUpdated = now
                        };
                        _db.user_availability_tbl.Add(newItem);
                    }
                }

                // Remove records not in the updated list
                var dayIDs = availabilities.Select(a => a.dayID).ToList();
                var toRemove = existingList.Where(x => !dayIDs.Contains(x.dayID)).ToList();

                if (toRemove.Any())
                    _db.user_availability_tbl.RemoveRange(toRemove);

                // Save all changes
                _db.SaveChanges();

                return ApiResponse<bool>.Ok(true, "Availability and slot duration saved successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail("Error saving availability: " + ex.Message);
            }
        }


    }
}