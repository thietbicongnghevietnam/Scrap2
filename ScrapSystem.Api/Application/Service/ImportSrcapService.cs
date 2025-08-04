using AutoMapper;
using ScrapSystem.Api.Application.DTOs.MaterialName;
using ScrapSystem.Api.Application.DTOs.ScrapDetailDtos;
using ScrapSystem.Api.Application.DTOs.ScrapDtos;
using ScrapSystem.Api.Application.Service.IServices;
using ScrapSystem.Api.Data.Repositories.IRepositories;
using ScrapSystem.Api.Domain.Models;
using ScrapSystem.Api.Application.Response;
using ScrapSystem.Api.Utilities;
using Serilog;
using System.Data;
using Microsoft.EntityFrameworkCore;
using ScrapSystem.Api.Application.DTOs.ScrapImageDtos;
using ScrapSystem.Api.Application.Request;
using ScrapSystem.Api.Application.DTOs.LabelListDtos;
using Microsoft.Data.SqlClient;


using Newtonsoft.Json;
using ScrapSystem.Api.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace ScrapSystem.Api.Application.Service
{
    public class ImportScrapService : IImportScrapService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;

        private readonly ExcelHelper _excelHelper;

        private readonly AppDbContext _dbContext;

        public ImportScrapService(IUnitOfWork unitOfWork, ExcelHelper excelHelper, IMapper mapper, AppDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _excelHelper = excelHelper;
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public async Task<ApiResult<bool>> UpdateQtyScrapDetail(int id, int qty, int QtyActual)
        {
            if (id <= 0)
            {
                return new ApiResult<bool>
                {
                    IsSuccess = false,
                    Message = "Invalid ScrapDetail ID."
                };
            }

            if (qty < 0)
            {
                return new ApiResult<bool>
                {
                    IsSuccess = false,
                    Message = "Quantity cannot be negative."
                };
            }

            if (QtyActual < 0)
            {
                return new ApiResult<bool>
                {
                    IsSuccess = false,
                    Message = "Quantity cannot be negative."
                };
            }

            var rs = await _unitOfWork.ScrapDetailRepository.UpdateScrapDetailById(id, qty, QtyActual);
            await _unitOfWork.SaveChangesAsync();

            return new ApiResult<bool>
            {
                IsSuccess = rs
            };
        }

        public async Task<ApiResult<bool>> DeleleScrapDetailById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return new ApiResult<bool>
                    {
                        IsSuccess = false,
                        Message = "Invalid ScrapDetail ID."
                    };
                }

                var scrap = await _unitOfWork.ScrapDetailRepository.Get(id);
                if (scrap == null)
                {
                    return new ApiResult<bool>
                    {
                        IsSuccess = false,
                        Message = "Cannot find scrap detail"
                    };
                }

                var rs = await _unitOfWork.ScrapDetailRepository.Delete(scrap);

                await _unitOfWork.SaveChangesAsync();

                return new ApiResult<bool>
                {
                    IsSuccess = true
                };
            }
            catch (Exception)
            {
                return new ApiResult<bool>
                {
                    IsSuccess = false
                };
                throw;
            }
            
        }

        public async Task<ApiResult<ParentWithChildren<ScrapDto, ScrapDetailDto>>> ImportScrapAsync(IFormFile file, string sanction, string section, string issueout)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return new ApiResult<ParentWithChildren<ScrapDto, ScrapDetailDto>>
                    {
                        IsSuccess = false,
                        Message = "Invalid or empty file."
                    };
                }

                string[] headers = new string[] { "SanctionID", "SubType", "MoveType", "IssueOutDate", "STT", "Plan", "Sloc", "CostCenter", "NameCost", "Material", "Quatity", "UnitPrice", "Amount", "Reason" };
                var data = _excelHelper.ExcelIssueOutToScrap(file, sanction, section);

                if (data.Item1 == null || data.Item2 == null || !data.Item2.Any())
                {
                    return new ApiResult<ParentWithChildren<ScrapDto, ScrapDetailDto>>
                    {
                        IsSuccess = false,
                        Message = "No valid data found in the Excel file."
                    };
                }

                //code new   31.07.2025

                var strategy = await _unitOfWork.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _unitOfWork.BeginTransactionAsync(); // mở transaction
                    try
                    {
                        var headerJson = JsonConvert.SerializeObject(data.Item1);
                        var detailJson = JsonConvert.SerializeObject(data.Item2);

                        var dbConnection = _dbContext.Database.GetDbConnection();
                        await using var command = dbConnection.CreateCommand();

                        command.CommandText = "ImportScrapAndDetails";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@Sanction", sanction));
                        command.Parameters.Add(new SqlParameter("@issueout", issueout));
                        command.Parameters.Add(new SqlParameter("@ScrapHeaderJson", headerJson));
                        command.Parameters.Add(new SqlParameter("@ScrapDetailsJson", detailJson));

                        if (dbConnection.State != ConnectionState.Open)
                            await dbConnection.OpenAsync();

                        // GÁN TRANSACTION HIỆN TẠI
                        command.Transaction = _dbContext.Database.CurrentTransaction?.GetDbTransaction();

                        await command.ExecuteNonQueryAsync();

                        await _unitOfWork.CommitTransactionAsync();

                        return new ApiResult<ParentWithChildren<ScrapDto, ScrapDetailDto>>
                        {
                            IsSuccess = true,
                            Message = "Import scrap successfully!",
                            MasterDetail = new ParentWithChildren<ScrapDto, ScrapDetailDto>
                            {
                                Parent = data.Item1,
                                Children = data.Item2
                            }
                        };
                    }
                    catch (Exception ex)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        Log.Error(ex, "Failed to import scrap for file {FileName}", file.FileName);
                        return new ApiResult<ParentWithChildren<ScrapDto, ScrapDetailDto>>
                        {
                            IsSuccess = false,
                            Message = ex.Message
                        };
                    }
                });

                //code old

                //var strategy = await _unitOfWork.CreateExecutionStrategy();
                //return await strategy.ExecuteAsync(async () =>
                //{
                //    using (var transaction = await _unitOfWork.BeginTransactionAsync())
                //    {
                //        try
                //        {

                //            Scrap scrap;
                //            var scrapExist = await _unitOfWork.ScrapRepository.GetScrapBySanctionAndStatusAsync(sanction, 0);

                //            if (scrapExist != null)
                //            {
                //                scrap = scrapExist;
                //                _mapper.Map(data.Item1, scrap);
                //                await _unitOfWork.ScrapRepository.Update(scrap);
                //            }
                //            else
                //            {
                //                scrap = _mapper.Map<Scrap>(data.Item1);
                //                await _unitOfWork.ScrapRepository.Add(scrap);
                //            }

                //            await _unitOfWork.SaveChangesAsync();

                //            var scrapDetails = _mapper.Map<List<ScrapDetail>>(data.Item2);
                //            foreach (var detail in scrapDetails)
                //            {
                //                detail.SanctionId = scrap.Id;
                //                if (scrapExist != null)
                //                    await _unitOfWork.ScrapDetailRepository.UpdateScrapDetail(detail);
                //                else
                //                    await _unitOfWork.ScrapDetailRepository.Add(detail);
                //            }

                //            await _unitOfWork.SaveChangesAsync();

                //            await _unitOfWork.CommitTransactionAsync();
                //            return new ApiResult<ParentWithChildren<ScrapDto, ScrapDetailDto>>
                //            {
                //                IsSuccess = true,
                //                Message = "Import scrap successfully!",
                //                MasterDetail = new ParentWithChildren<ScrapDto, ScrapDetailDto>
                //                {
                //                    Parent = data.Item1,
                //                    Children = data.Item2
                //                }
                //            };

                //        }
                //        catch (Exception ex)
                //        {
                //            await _unitOfWork.RollbackTransactionAsync();
                //            Log.Error(ex, "Failed to import scrap for file {FileName}", file.FileName);
                //            return new ApiResult<ParentWithChildren<ScrapDto, ScrapDetailDto>>
                //            {
                //                IsSuccess = false,
                //                Message = ex.Message
                //            };
                //        }
                //    }
                //});

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return new ApiResult<ParentWithChildren<ScrapDto, ScrapDetailDto>>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResult<MaterialNameDto>> ImportMaterialNameAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return new ApiResult<MaterialNameDto>
                    {
                        IsSuccess = false,
                        Message = "Invalid or empty file."
                    };
                }

                string[] headers = new string[] { "PartNo", "EnglishName", "VietNameseName" };
                List<MaterialNameDto> materialNameDtos = _excelHelper.ExcelToMaterialName(file);

                var materialNames = _mapper.Map<List<MaterialName>>(materialNameDtos);
                var data = materialNames.DistinctBy(x => x.Material).ToList();

                await _unitOfWork.MaterialNameRepository.AddMultiEntities(materialNames.DistinctBy(x => x.Material).ToList());
                await _unitOfWork.SaveChangesAsync();
                return new ApiResult<MaterialNameDto>
                {
                    IsSuccess = true,
                    Message = "Import scrap successfully !",
                    Items = materialNameDtos
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);

                return new ApiResult<MaterialNameDto>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }

        }

        public async Task<ApiResult<ScrapViewDto>> LoadData(ScrapRequest request)
        {
            try
            {
                var rs = await _unitOfWork.ScrapRepository.GetReportScrapByDate(request.StartDate, request.EndDate, request.Sanction, request.Status, request.Page, request.PageSize);

                return new ApiResult<ScrapViewDto>
                {
                    IsSuccess = true,
                    Message = "Retrieved scrap data successfully.",
                    PagedResult = new PaginatedResult<ScrapViewDto>
                    {
                        Records = rs.Data,
                        TotalCount = rs.TotalCount
                    }
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);

                return new ApiResult<ScrapViewDto>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="barCode">sanctionId;pallet</param>
        /// <returns></returns>
        public async Task<ApiResult<MasterDetailDto<ScrapImageDto, ScrapImageDetailDto>>> LoadImage(string sanctionId, string pallet)
        {
            try
            {
                var parameters = new { Sanction = sanctionId, Pallet = pallet };

                var (imageScraps, imageScrapDetails) = await _unitOfWork.ImageScrapRepository.ExecuteStoredProcedureMultiDataAsync<ScrapImageDto, ScrapImageDetailDto>(
                    "GetImageScrap",
                    parameters);
                return new ApiResult<MasterDetailDto<ScrapImageDto, ScrapImageDetailDto>>
                {
                    IsSuccess = true,
                    MasterDetail = new MasterDetailDto<ScrapImageDto, ScrapImageDetailDto>
                    {
                        Masters = imageScraps,
                        Details = imageScrapDetails
                    }
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);

                return new ApiResult<MasterDetailDto<ScrapImageDto, ScrapImageDetailDto>>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResult<MasterDetailDto<LabelListMasterDto, LabelListDetailDto>>> GetLabelListAsync(DateTime startDate, DateTime endDate, string sanction)
        {
            try
            {
                if (startDate == DateTime.MinValue || endDate == DateTime.MinValue || startDate > endDate)
                {
                    return new ApiResult<MasterDetailDto<LabelListMasterDto, LabelListDetailDto>>
                    {
                        IsSuccess = false,
                        Message = "Invalid date range."
                    };
                }

                var parameters = new { StartDate = startDate, EndDate = endDate, Sanction = sanction };

                var (master, details) = await _unitOfWork.ImageScrapRepository.ExecuteStoredProcedureMultiDataAsync<LabelListMasterDto, LabelListDetailDto>(
                    "GetBarcodes",
                    parameters);


                return new ApiResult<MasterDetailDto<LabelListMasterDto, LabelListDetailDto>>
                {
                    IsSuccess = true,
                    MasterDetail = new MasterDetailDto<LabelListMasterDto, LabelListDetailDto>
                    {
                        Masters = master,
                        Details = details
                    }
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while executing the stored procedure.");

                return new ApiResult<MasterDetailDto<LabelListMasterDto, LabelListDetailDto>>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }

        }

        public async Task<ApiResult<byte[]>> PrintLabelList(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate == DateTime.MinValue || endDate == DateTime.MinValue || startDate > endDate)
                {
                    return new ApiResult<byte[]>
                    {
                        IsSuccess = false,
                        Message = "Invalid date range."
                    };
                }

                var parameters = new { StartDate = startDate, EndDate = endDate };

                var (master, details) = await _unitOfWork.ImageScrapRepository.ExecuteStoredProcedureMultiDataAsync<LabelListMasterDto, LabelListDetailDto>(
                    "GetBarcodes",
                    parameters);


                return new ApiResult<byte[]>
                {
                    IsSuccess = true,

                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while executing the stored procedure.");

                return new ApiResult<byte[]>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }

        }

        //Task<ApiResult<bool>> IImportScrapService.UpdateQtyScrapDetail(int id, int qty, int QtyActual)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
