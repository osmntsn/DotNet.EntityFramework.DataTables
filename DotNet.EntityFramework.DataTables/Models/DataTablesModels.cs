using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTables.AspNetCore.Models
{
    public enum SortingDirection { ASC = 0, DESC = 1 };
    public enum FilterType { Equal = 0, NotEqual, Greater, GreaterOrEqual, Less, LessOrEqual, OrderBy };

    public class FilterDataModel
    {
        public string filterFor { get; set; }
        public string fiterType { get; set; }
        public string value { get; set; }
        public string dataType { get; set; }
    }

    public class FilterDataConvertedModel
    {
        public string filterFor { get; set; }
        public FilterType fiterType { get; set; }
        public object value { get; set; }
        public Type dataType { get; set; }
        public FilterDataConvertedModel(FilterDataModel model)
        {
            this.filterFor = model.filterFor;
            this.fiterType = GetFilteType(model.fiterType);
            if (model.dataType == "DateTime")
            {
                this.value = AspNetCoreHelpers.ControlDate(model.value, DateTime.Now);
                this.dataType = typeof(DateTime?);
            }
            else if (model.dataType == "int")
            {
                this.value = Convert.ToInt32(model.value);
                this.dataType = typeof(int?);
            }
            else if (model.dataType == "short")
            {
                this.value = Convert.ToInt16(model.value);
                this.dataType = typeof(Int16?);
            }
            else if (model.dataType == "number")
            {
                this.value = Convert.ToDouble(model.value);
                this.dataType = typeof(double?);
            }
            else if (model.dataType == "decimal")
            {
                this.value = Convert.ToDouble(model.value);
                this.dataType = typeof(decimal?);
            }
            else if (model.dataType == "bool")
            {
                this.value = Convert.ToBoolean(model.value.ToLowerInvariant());
                this.dataType = typeof(bool?);
            }
            else
            {
                this.value = model.value;
                this.dataType = typeof(string);
            }
        }
        public FilterType GetFilteType(string filterType)
        {
            switch (filterType)
            {
                case "equal": return FilterType.Equal;
                case "notEqual": return FilterType.NotEqual;
                case "greater": return FilterType.Greater;
                case "greaterOrEqual": return FilterType.GreaterOrEqual;
                case "less": return FilterType.Less;
                case "lessOrEqual": return FilterType.LessOrEqual;
                default: return FilterType.Equal;
            }
        }
    }
    public class PaginateTableViewModel
    {
        public DatatablesSettings datatablesSettings { get; set; }
        public Type type { get; set; }
        public string url { get; set; }
        public int datatableNo { get; set; }
        public DatatablesDataModel dataTablesModel { get; set; }
        public List<KeyValuePair<string, string>> listProperties { get; set; }
        public IQueryable ExcelQuery { get; set; }
        public bool ExcelActive { get; set; }
        public bool ExcelTotalActive { get; set; }
        public bool ExcelAverageActive { get; set; }
        public string ExcelName { get; set; }
        public string ExcelTableName { get; set; }
        public FilterDataModel[] filters { get; set; }
        public string[] filterExludeColumns { get; set; }
        public string[] excelExludeColumns { get; set; }
        public List<string> hiddenColumns { get; set; }
        public bool FilterChangeActive = true;
        public bool IsTotalActive = false;

        public PaginateTableViewModel()
        {
            listProperties = new List<KeyValuePair<string, string>>();
            ExcelActive = false;
            ExcelAverageActive = false;
            ExcelTotalActive = false;
            DateColumsList = new List<string>();
            hiddenColumns = new List<string>();
        }
        public List<string> DateColumsList { get; set; }
        public string Style { get; set; }
        public string Class { get; set; }
        public string CountSelector = "";
    }
    public class DatatablesSettings
    {
        public int iDisplayLength { get; set; }
        public int iDisplayStart { get; set; }
        public string sSearch { get; set; }
        public int iSortCol { get; set; }
        public SortingDirection iSortDir { get; set; }
        public bool returnJson { get; set; }
    }

    public class DatatablesDataModel
    {
        public int iTotalRecords { get; set; }
        public int iTotalDisplayRecords { get; set; }
        public System.Collections.IList aaData { get; set; }
        public List<DataTableFooterModel> footerDataList { get; set; }

    }
    public class DataTableFooterModel
    {
        public int Index { get; set; }
        public string Value { get; set; }
    }
    public class DataTableJsonModel
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class PaginateFilterModel<T>
    {
        public IQueryable<T> Query { get; set; }
        public int Count { get; set; }
        public int FilteredCount { get; set; }
    }
}
