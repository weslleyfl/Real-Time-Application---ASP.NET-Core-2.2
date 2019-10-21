using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ASC.Models.BaseTypes;
using ASC.Business.Interfaces;
using AutoMapper;
using ASC.Models.Models;
using ASC.Web.Models.MasterDataViewModels;
using ASC.Utilities;
using System.IO;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using ASC.Web.Data.Cache;

namespace ASC.Web.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class MasterDataController : BaseController
    {
        private readonly IMasterDataOperations _masterData;
        private readonly IMapper _mapper;
        private readonly IMasterDataCacheOperations _masterDataCache;

        public MasterDataController(IMasterDataOperations masterData, IMapper mapper, IMasterDataCacheOperations masterDataCache)
        {
            _masterData = masterData;
            _mapper = mapper;
            _masterDataCache = masterDataCache;
        }

        [HttpGet]
        public async Task<IActionResult> MasterKeys()
        {
            var masterKeys = await _masterData.GetAllMasterKeysAsync();
            var masterKeysViewModel = _mapper.Map<List<MasterDataKey>, List<MasterDataKeyViewModel>>(masterKeys);

            // Hold all master key in session
            HttpContext.Session.SetSession("MasterKeys", masterKeysViewModel);

            return View(new MasterKeysViewModel
            {
                MasterKeys = masterKeysViewModel == null ? null : masterKeysViewModel.ToList(),
                IsEdit = false
            });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterKeys(MasterKeysViewModel masterKeysView)
        {
            masterKeysView.MasterKeys = HttpContext.Session.GetSession<List<MasterDataKeyViewModel>>("MasterKeys");

            if (!ModelState.IsValid)
            {
                return View(masterKeysView);
            }

            var masterKey = _mapper.Map<MasterDataKeyViewModel, MasterDataKey>(masterKeysView.MasterKeyInContext);

            if (masterKeysView.IsEdit)
            {
                // Update master key
                await _masterData.UpdateMasterKeyAsync(masterKeysView.MasterKeyInContext.PartitionKey, masterKey);
            }
            else
            {
                // Insert Master Key
                masterKey.RowKey = Guid.NewGuid().ToString();
                masterKey.PartitionKey = masterKey.Name;
                await _masterData.InsertMasterKeyAsync(masterKey);
            }

            await _masterDataCache.CreateMasterDataCacheAsync();

            return RedirectToAction("MasterKeys");

        }

        [HttpGet]
        public async Task<IActionResult> MasterValues()
        {
            // Get All Master Keys and hold them in ViewBag for Select tag
            ViewBag.MasterKeys = (await _masterData.GetAllMasterKeysAsync()).Where(k => k.IsActive).ToList();

            return View(new MasterValuesViewModel
            {
                MasterValues = new List<MasterDataValueViewModel>(),
                IsEdit = false
            });
        }

        [HttpGet]
        public async Task<IActionResult> MasterValuesByKey(string key)
        {
            // Get Master values based on master key.
            return Json(new { data = await _masterData.GetAllMasterValuesByKeyAsync(key) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterValues(bool isEdit, MasterDataValueViewModel masterValue)
        {
            if (!ModelState.IsValid) return Json("Error");

            var masterDataValue = _mapper.Map<MasterDataValueViewModel, MasterDataValue>(masterValue);

            if (isEdit)
            {
                // Update Master Value
                await _masterData.UpdateMasterValueAsync(masterDataValue.PartitionKey, masterDataValue.RowKey, masterDataValue);
            }
            else
            {
                // Insert Master Value
                masterDataValue.RowKey = Guid.NewGuid().ToString();
                await _masterData.InsertMasterValueAsync(masterDataValue);
            }

            await _masterDataCache.CreateMasterDataCacheAsync();

            return Json("True");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadExcel()
        {
            var files = Request.Form.Files;
            // Validations
            if (!files.Any())
            {
                return Json(new { Error = true, Text = "Upload a file" });
            }

            var excelFile = files.First();
            if (excelFile.Length <= 0)
            {
                return Json(new { Error = true, Text = "Upload a file" });
            }

            if (!Path.GetExtension(excelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { Error = true, Text = "Not Support file extension" });
            }

            // Parse Excel Data
            var masterData = await ParseMasterDataExcel(excelFile);
            var result = await _masterData.UploadBulkMasterData(masterData);

            await _masterDataCache.CreateMasterDataCacheAsync();

            return Json(new { Success = result });
        }

        private async Task<List<MasterDataValue>> ParseMasterDataExcel(IFormFile excelFile)
        {
            var masterValueList = new List<MasterDataValue>();
            using (var memoryStream = new MemoryStream())
            {
                // Get MemoryStream from Excel file
                await excelFile.CopyToAsync(memoryStream);
                // Create a ExcelPackage object from MemoryStream
                using (ExcelPackage package = new ExcelPackage(memoryStream))
                {
                    // Get the first Excel sheet from the Workbook
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                    int rowCount = worksheet.Dimension.Rows;

                    // Iterate all the rows and create the list of MasterDataValue
                    // Ignore first row as it is header
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var masterDataValue = new MasterDataValue();
                        masterDataValue.RowKey = Guid.NewGuid().ToString();
                        masterDataValue.PartitionKey = worksheet.Cells[row, 1].Value.ToString();
                        masterDataValue.Name = worksheet.Cells[row, 2].Value.ToString();
                        masterDataValue.IsActive = Boolean.Parse(worksheet.Cells[row, 3].Value.ToString());

                        masterValueList.Add(masterDataValue);
                    }
                }
            }
            return masterValueList;
        }

    }
}