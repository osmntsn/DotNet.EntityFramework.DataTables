using DataTables.AspNetCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTables.AspNetCore.Helpers
{
    public static class AspnetCoreDataTableCreator
    {
        public static IViewComponentResult ReturnPaginationResult(PaginateTableViewModel model)
        {
            /*if (isExcelRequest() && model.ExcelActive)
            {
                SortedDictionary<string, string> Headerlist = new SortedDictionary<string, string>();
                model.listProperties.ForEach(x => Headerlist.Add(x.Key, x.Value));
                return model.ExcelQuery.UnsafeCast<IQueryable<object>>().ToExcelFile(model.ExcelName, !String.IsNullOrEmpty(model.ExcelTableName) ? model.ExcelTableName : model.ExcelName, Headerlist, exludeColumnsList: model.excelExludeColumns, AddTotalRow: model.ExcelTotalActive, AddAverageRow: model.ExcelAverageActive);
            }
            else*/
            if (model.datatablesSettings.returnJson)
            {
                return new HtmlContentViewComponentResult(new Microsoft.AspNetCore.Html.HtmlString(JsonConvert.SerializeObject(model.dataTablesModel)));
            }
            else
            {
                return new ViewViewComponentResult
                {
                    ViewName = "~/Areas/DataTables/Views/_PaginateTable.cshtml",
                    ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                    {
                        Model = model,
                    }
                };
            }
        }

        public static IActionResult ReturnPaginationResultView(PaginateTableViewModel model, HttpRequest request)
        {
            if (IsExcelRequest(request) && model.ExcelActive)
            {
                SortedDictionary<string, string> Headerlist = new SortedDictionary<string, string>();
                model.listProperties.ForEach(x => Headerlist.Add(x.Key, x.Value));
                return model.ExcelQuery.UnsafeCast<IQueryable<object>>().ToExcelFile(model.ExcelName, !String.IsNullOrEmpty(model.ExcelTableName) ? model.ExcelTableName : model.ExcelName, Headerlist, exludeColumnsList: model.excelExludeColumns, AddTotalRow: model.ExcelTotalActive, AddAverageRow: model.ExcelAverageActive);
            }
            else
            if (model.datatablesSettings.returnJson)
            {
                return new ContentResult { Content = JsonConvert.SerializeObject(model.dataTablesModel), ContentType = "application/json", StatusCode = 200 };
                //return new ContentResult(new Microsoft.AspNetCore.Html.HtmlString(JsonConvert.SerializeObject(model.dataTablesModel)));
            }
            else
            {
                return new ViewResult
                {
                    ViewName = "~/Areas/DataTables/Views/_PaginateTable.cshtml",
                    ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                    {
                        Model = model,
                    }
                };
            }
        }

        public static bool IsExcelRequest(HttpRequest request)
        {
            try
            {
                if (request != null)
                {
                    return request.Query["Excel"].ToString() == "true";
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
