using DataTables.AspNetCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTables.AspNetCore
{
    public static class AspNetCoreHelpers
    {
        public static DateTime ControlDate(string Date, DateTime DefaultDate)
        {
            try
            {
                if (!String.IsNullOrEmpty(Date))
                {
                    DateTime time = Convert.ToDateTime(Date);
                    return time;
                }
                else return DefaultDate;
            }
            catch (Exception)
            {
                return DefaultDate;
            }
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
    }
}
