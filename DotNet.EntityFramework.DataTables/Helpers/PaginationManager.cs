using DataTables.AspNetCore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DataTables.AspNetCore
{
    public static class PaginationManager
    {
        public static string GetFromFilterData(ref FilterDataModel[] filterData, string key, bool remove)
        {
            if (filterData == null)
                return null;

            var filterItem = filterData.FirstOrDefault(x => x.filterFor == key);

            if (filterItem == null)
                return null;

            if (remove)
            {
                filterData = filterData.Where(x => x.filterFor != key).ToArray();
            }

            return filterItem.value;
        }
        public static FilterDataModel[] ParseRangeFilters(FilterDataModel[] filterData)
        {
            if (filterData != null)
            {
                List<FilterDataModel> filterList = filterData.ToList();
                List<FilterDataModel> RemoveList = new List<FilterDataModel>();
                List<FilterDataModel> AddList = new List<FilterDataModel>();

                foreach (var filter in filterList.Where(x => x.fiterType == "Range"))
                {
                    if (!String.IsNullOrEmpty(filter.value) && filter.value.Contains("-"))
                    {
                        RemoveList.Add(filter);
                        AddList.Add(new FilterDataModel
                        {
                            dataType = filter.dataType,
                            filterFor = filter.filterFor,
                            fiterType = "greaterOrEqual",
                            value = filter.value.Split('-')[0].Trim(),
                        });

                        AddList.Add(new FilterDataModel
                        {
                            dataType = filter.dataType,
                            filterFor = filter.filterFor,
                            fiterType = "lessOrEqual",
                            value = filter.value.Split('-')[1].Trim(),
                        });
                    }
                }
                RemoveList.ForEach(x => filterList.Remove(x));
                filterList.AddRange(AddList);
                return filterList.ToArray();
            }
            else return null;
        }

        public static T UnsafeCast<T>(this IQueryable obj)
        {
            return (T)obj;
        }

        class OrderingMethodFinder : ExpressionVisitor
        {
            bool _orderingMethodFound = false;

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                var name = node.Method.Name;

                if (node.Method.DeclaringType == typeof(Queryable) && (
                    name.StartsWith("OrderBy", StringComparison.Ordinal) ||
                    name.StartsWith("ThenBy", StringComparison.Ordinal)))
                {
                    _orderingMethodFound = true;
                }

                return base.VisitMethodCall(node);
            }

            public static bool OrderMethodExists(Expression expression)
            {
                var visitor = new OrderingMethodFinder();
                visitor.Visit(expression);
                return visitor._orderingMethodFound;
            }
        }

        public static IQueryable<T> FilterPaginateQuery<T>(this IQueryable<T> list, DatatablesSettings setttings, FilterDataModel[] filters, string[] filterExludeColumns, bool DisableFilter = false)
        {
            var properties = typeof(T).GetProperties();

            if (filters != null && filters.Any(x => x.fiterType == "OrderBy" && x.value != "-1"))
            {
                var OrderByFilter = filters.First(x => x.fiterType == "OrderBy" && x.value != "-1").value;
                var OrderByFilterName = OrderByFilter.Split('|')[0];
                var OrderByFilterDirection = OrderByFilter.Split('|').Length > 0 ? OrderByFilter.Split('|')[1] : "";

                if (String.IsNullOrEmpty(OrderByFilterDirection) || OrderByFilterDirection == "asc")
                    list = list.OrderBy(OrderByFilterName);
                else
                    list = list.OrderByDescending(OrderByFilterName);

                var tmpFilters = filters.ToList();
                tmpFilters.RemoveAll(x => x.fiterType == "OrderBy");
                filters = tmpFilters.ToArray();
            }
            else
            {
                if (!DisableFilter)
                {
                    if (setttings.iSortDir == SortingDirection.DESC)
                        list = list.OrderByDescending(typeof(T).GetProperties().ElementAt(setttings.iSortCol).Name);
                    else
                        list = list.OrderBy(typeof(T).GetProperties().ElementAt(setttings.iSortCol).Name);
                }
            }

            if (!String.IsNullOrEmpty(setttings.sSearch))
                list = list.WhereContains(properties.Where(x => !filterExludeColumns.Contains(x.Name) && !x.PropertyType.FullName.ToLowerInvariant().Contains("bool")).Select(x => x.Name).ToArray(), setttings.sSearch);

            if (filters != null && filters.Count() > 0)
            {
                foreach (var item in filters)
                {
                    if (item.value != "-1")
                    {
                        var filter = new FilterDataConvertedModel(item);
                        list = list.WhereFilter(filter.filterFor, filter.value, filter.dataType, filter.fiterType);
                    }
                }
            }
            return list;
        }

        private static DatatablesDataModel GetFilteredList<T>(IQueryable<T> list, DatatablesSettings setttings, FilterDataModel[] filters, string[] filterExludeColumns, PaginateTableViewModel MainModel, int? Count = 0, bool SkipDisabled = false, int? FilteredCount = null, bool DisableFilter = false)
        {
            int count = Count == null ? list.Count() : (int)Count;
            list = list.FilterPaginateQuery<T>(setttings, filters, filterExludeColumns);
            FilteredCount = FilteredCount == null ? list.Count() : (int)FilteredCount;
            MainModel.ExcelQuery = list;

            DatatablesDataModel model = new DatatablesDataModel
            {
                aaData = setttings.returnJson ? (!SkipDisabled ? list.Skip(setttings.iDisplayStart).Take(setttings.iDisplayLength).ToList() : list.ToList()) : null,
                iTotalRecords = (int)FilteredCount,// count,
                iTotalDisplayRecords = (int)FilteredCount,
            };
            return model;
        }

        public static PaginateFilterModel<T> PaginateFilter<T>(this IQueryable<T> list, string settings, FilterDataModel[] filters, int DefaultOrderbyColumnIndex = 0, SortingDirection DefaultSortingDirection = SortingDirection.ASC, int DatatableNo = 1, bool ExcelActive = false, bool DisableSkip = false, bool ReturnAsList = true, params string[] filterExludeColumns)
        {
            filters = AspNetCoreHelpers.ParseRangeFilters(filters);
            PaginateTableViewModel model = new PaginateTableViewModel();
            model.filters = filters;
            model.filterExludeColumns = filterExludeColumns;
            model.datatablesSettings = GetDatatableSettings(settings, DefaultOrderbyColumnIndex, DefaultSortingDirection, false, true);
            model.type = typeof(T);
            int Count = list.Count();
            var FilteredQuery = list.FilterPaginateQuery<T>(model.datatablesSettings, filters, filterExludeColumns);
            int FilteredCount = FilteredQuery.Count();

            return new PaginateFilterModel<T>
            {
                Count = Count,
                FilteredCount = FilteredCount,
                Query = !(/*(isExcelRequest() && ExcelActive) ||*/ DisableSkip) ?
                ReturnAsList ?
                FilteredQuery.Skip(model.datatablesSettings.iDisplayStart).Take(model.datatablesSettings.iDisplayLength).ToList().AsQueryable() : FilteredQuery.Skip(model.datatablesSettings.iDisplayStart).Take(model.datatablesSettings.iDisplayLength).AsQueryable()
                :
                ReturnAsList ?
                FilteredQuery.ToList().AsQueryable() : FilteredQuery.AsQueryable()
            };
        }

        public static PaginateTableViewModel Paginate<T>(this IQueryable<T> list, string settings, FilterDataModel[] filters, string url, int DefaultOrderbyColumnIndex = 0, SortingDirection DefaultSortingDirection = SortingDirection.ASC, int DatatableNo = 1, bool ExcelActive = false, bool DisableFilter = false, int? PaginateCount = null, int? FilteredCount = null, params string[] filterExludeColumns)
        {
            filters = AspNetCoreHelpers.ParseRangeFilters(filters);
            PaginateTableViewModel model = new PaginateTableViewModel();
            model.filters = filters;
            model.filterExludeColumns = filterExludeColumns;
            model.datatablesSettings = GetDatatableSettings(settings, DefaultOrderbyColumnIndex, DefaultSortingDirection, DisableSearch: DisableFilter);
            model.type = typeof(T);
            model.dataTablesModel = GetFilteredList(list, model.datatablesSettings, !DisableFilter ? filters : null, filterExludeColumns, model, PaginateCount, PaginateCount != null, FilteredCount);
            model.url = url;
            model.datatableNo = DatatableNo;
            model.ExcelActive = ExcelActive;
            return model;
        }

        public static DatatablesSettings GetDatatableSettings(string settings, int DefaultOrderbyColumnIndex = 0, SortingDirection DefaultSortingDirection = SortingDirection.ASC, bool DisableSearch = false, bool ForceOrderby = false)
        {
            if (!string.IsNullOrEmpty(settings))
            {
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(settings)))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<DataTableJsonModel>));
                    List<DataTableJsonModel> list = (List<DataTableJsonModel>)serializer.ReadObject(ms);
                    DatatablesSettings parsedSettings = new DatatablesSettings
                    {
                        iDisplayStart = Convert.ToInt32(list.First(x => x.name == "iDisplayStart").value) >= 0 ? Convert.ToInt32(list.First(x => x.name == "iDisplayStart").value) : 0,
                        iDisplayLength = Convert.ToInt32(list.First(x => x.name == "iDisplayLength").value) >= 0 ? Convert.ToInt32(list.First(x => x.name == "iDisplayLength").value) : 10,
                        sSearch = list.Any(x => x.name == "sSearch") && !DisableSearch ? list.First(x => x.name == "sSearch").value : "",
                        iSortCol = ForceOrderby ? DefaultOrderbyColumnIndex : list.Any(x => x.name == "iSortCol_0") ? Convert.ToInt32(list.First(x => x.name == "iSortCol_0").value) : 0,
                        iSortDir = ForceOrderby ? DefaultSortingDirection : list.Any(x => x.name == "sSortDir_0") ? (list.First(x => x.name == "sSortDir_0").value == "asc" ? SortingDirection.ASC : SortingDirection.DESC) : SortingDirection.ASC,
                        returnJson = true,
                    };
                    parsedSettings.iDisplayStart = parsedSettings.iDisplayStart < 0 ? 0 : parsedSettings.iDisplayStart;
                    return parsedSettings;
                }
            }
            else
            {
                return new DatatablesSettings { iDisplayLength = 10, iDisplayStart = 0, sSearch = "", iSortCol = DefaultOrderbyColumnIndex, iSortDir = DefaultSortingDirection, returnJson = false, };
            }
        }
    }
}
