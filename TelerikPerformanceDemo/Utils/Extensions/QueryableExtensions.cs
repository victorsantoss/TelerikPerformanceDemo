using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.Infrastructure;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using TelerikPerformanceDemo.Models;

namespace TelerikPerformanceDemo.Utils.Extensions
{
    /// <summary>
    /// basé sur https://demos.telerik.com/aspnet-mvc/grid/customserverbinding
    /// </summary>
    public static class QueryableExtensions
    {

    
        public static DataSourceResult ToCustomDataSourceResul(this IQueryable<OrderDetailViewModel> source, DataSourceRequest request)
        {
            var watch = Stopwatch.StartNew();

            //DataSourceResult result = AppliquerGroupePaginationAlternative(source, request);
            //========================== AppliquerGroupePaginationAlternative

            var grouping = request.RetirerGrouping(source);
            var page = request.Page;
            var pageSize = request.PageSize;

            //Remove the grouping before exeecuting the query against NHibernate
            
            request.Page = 0;
            request.PageSize = 0;

            var total = source.Count();
            var result = source.ToDataSourceResult(request);

           
            var skip = page > 0 ? (page - 1) * pageSize : 0;
            result.Data = result.Data.AsQueryable().Skip(skip).Take(pageSize);

            //Réappliquer le grouping
            request.Groups = grouping;
            result = result.Data.ToDataSourceResult(request);
            result.Total = total;


            //==========================

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            Debug.WriteLine("============================================================");
            Debug.WriteLine("CustomDataSourceResult: " + elapsedMs + " ms");
            Debug.WriteLine("============================================================");

            return result;
        }


        public static IList<GroupDescriptor> RetirerGrouping(this DataSourceRequest request, IQueryable<OrderDetailViewModel> source)
        {
            var grouping = request.Groups;

            //convertir les grouping en OrderBy
            var tempSort = grouping.Select(g => new SortDescriptor()
            {
                Member = g.Member,
                SortDirection = g.SortDirection
            }).ToList();
            
            tempSort.Reverse();
            tempSort.ForEach(x => request.Sorts.Insert(0, x));
            request.Groups = null;
            return grouping;
        }

        //private static DataSourceResult AppliquerGroupePagination(IQueryable<OrderDetailViewModel> source, DataSourceRequest request)
        //{
        //    var selector = source.VerifyOrdersGrouping(request.Groups);
        //    var total = source.Count();
        //    IEnumerable data;
        //    if (selector != null)
        //    {
        //        var skip = request.Page > 0 ? (request.Page - 1) * request.PageSize : 0;
        //        data = selector.Invoke(source).Skip(skip).Take(request.PageSize).ToList();
        //    }
        //    else
        //    {
        //        data = source.ApplyOrdersPaging(request.Page, request.PageSize).ToList();
        //    }

        //    var result = new DataSourceResult()
        //    {
        //        Data = data,
        //        Total = total
        //    };
        //    return result;
        //}

        //private static DataSourceResult AppliquerGroupePaginationAlternative(IQueryable<OrderDetailViewModel> source, DataSourceRequest request)
        //{

        //    var grouping = request.Groups;

        //    //convertir les grouping en trie
        //    //TODO: ici il faut regarder dans les sort pour appliquer les même sort.
        //    var tempSort = grouping.Select(g => new SortDescriptor()
        //    {
        //        Member = g.Member,
        //        SortDirection = g.SortDirection
        //    }).ToList();


        //    tempSort.Reverse();
        //    tempSort.ForEach(x => request.Sorts.Insert(0, x));


        //    var page = request.Page;
        //    var pageSize = request.PageSize;

        //    //Remove the grouping before exeecuting the query against NHibernate
        //    request.Groups = null;
        //    request.Page = 0;
        //    request.PageSize = 0;

        //    //Execute the query and return results from the database.
        //    //These results will be paged, filtered and ordered.
        //    var total = source.Count();
        //    var result = source.ToDataSourceResult(request);


        //    var skip = page > 0 ? (page - 1) * pageSize : 0;
        //    result.Data = result.Data.AsQueryable().Skip(skip).Take(pageSize);

        //    //Réappliquer le grouping
        //    request.Groups = grouping;
        //    result = result.Data.ToDataSourceResult(request);
        //    result.Total = total;

        //    return result;
        //}

        //public static DataSourceResult ToCustomDataSourceResult(this IQueryable<OrderDetailViewModel> source, DataSourceRequest request)
        //{
        //    var watch = System.Diagnostics.Stopwatch.StartNew();

        //    source = source.ApplyOrdersFiltering(request.Filters);

        //    var total = source.Count();

        //    source = source.ApplyOrdersSorting(request.Groups, request.Sorts);

        //    if (!request.Sorts.Any())
        //    {
        //        source = source.OrderBy(o => o.OrderDetailID);
        //    }

        //    var selector = source.VerifyOrdersGrouping(request.Groups);

        //    IEnumerable data;
        //    if (selector != null)
        //    {
        //        var skip = request.Page > 0 ? (request.Page - 1) * request.PageSize : 0;
        //        data = selector.Invoke(source).Skip(skip).Take(request.PageSize).ToList();
        //    }
        //    else
        //    {
        //        data = source.ApplyOrdersPaging(request.Page, request.PageSize).ToList();
        //    }

        //    var result = new DataSourceResult()
        //    {
        //        Data = data,
        //        Total = total
        //    };

        //    watch.Stop();
        //    var elapsedMs = watch.ElapsedMilliseconds;

        //    Debug.WriteLine("============================================================");
        //    Debug.WriteLine("CustomDataSourceResult: " + elapsedMs + " ms");
        //    Debug.WriteLine("============================================================");

        //    return result;
        //}

        //public static DataSourceResult ToCustomDataSourceResult2(this IQueryable<OrderDetailViewModel> source, DataSourceRequest request)
        //{
        //    var watch = System.Diagnostics.Stopwatch.StartNew();

        //    source = source.ApplyOrdersFiltering(request.Filters);

        //    var total = source.Count();

        //    source = source.ApplyOrdersSorting(request.Groups, request.Sorts);

        //    if (!request.Sorts.Any())
        //    {
        //        source = source.OrderBy(o => o.OrderDetailID);
        //    }

        //    var selector = source.VerifyOrdersGrouping(request.Groups);

        //    IEnumerable data;
        //    if (selector != null)
        //    {
        //        var skip = request.Page > 0 ? (request.Page - 1) * request.PageSize : 0;
        //        //data = selector.Invoke(source).Skip(skip).Take(request.PageSize).ToList();

        //        data = selector.Invoke(source).ToList();

        //        List<AggregateFunctionsGroup> aggregateFunctionsGroupList = new List<AggregateFunctionsGroup>();

        //        //foreach (AggregateFunctionsGroup item in data)
        //        //{
        //        //    aggregateFunctionsGroupList.Add(item);

        //        //    if(aggregateFunctionsGroupList.Count >10) break;

        //        //}
        //        //data = aggregateFunctionsGroupList;



        //    }
        //    else
        //    {
        //        data = source.ApplyOrdersPaging(request.Page, request.PageSize).ToList();
        //    }

        //    var result = new DataSourceResult()
        //    {
        //        Data =  data,
        //        Total = total
        //    };

        //    watch.Stop();
        //    var elapsedMs = watch.ElapsedMilliseconds;

        //    Debug.WriteLine("============================================================");
        //    Debug.WriteLine("CustomDataSourceResult: " + elapsedMs + " ms");
        //    Debug.WriteLine("============================================================");

        //    return result;
        //}

        //public static DataSourceResult ToCustomDataSourceResultAlt(this IQueryable<OrderDetailViewModel> source, DataSourceRequest request)
        //{
        //    var watch = System.Diagnostics.Stopwatch.StartNew();

        //    // FILTRE
        //    source = source.ApplyOrdersFiltering(request.Filters);

        //    // TRIER
        //    source = source.ApplyOrdersSorting(request.Groups, request.Sorts);

        //    // TRIER DEFAUT
        //    if (!request.Sorts.Any())
        //    {
        //        source = source.OrderBy(o => o.OrderDetailID);
        //    }

        //    // GROUPE / AGGREGATE
        //    DataSourceResult result = AppliquerGroupePaginationAlternative(source, request);

        //    watch.Stop();
        //    var elapsedMs = watch.ElapsedMilliseconds;

        //    Debug.WriteLine("============================================================");
        //    Debug.WriteLine("CustomDataSourceResult: " + elapsedMs + " ms");
        //    Debug.WriteLine("============================================================");

        //    return result;
        //}

    }

    public static class AjaxCustomBindingExtensions
    {
        public static IQueryable<OrderDetailViewModel> ApplyOrdersPaging(this IQueryable<OrderDetailViewModel> data, int page, int pageSize)
        {
            if (pageSize > 0 && page > 0)
            {
                data = data.Skip((page - 1) * pageSize);
            }

            data = data.Take(pageSize);

            return data;
        }

        public static Func<IEnumerable<OrderDetailViewModel>, IEnumerable<AggregateFunctionsGroup>> VerifyOrdersGrouping(this IQueryable<OrderDetailViewModel> data, IList<GroupDescriptor>
            groupDescriptors)
        {
            Func<IEnumerable<OrderDetailViewModel>, IEnumerable<AggregateFunctionsGroup>> selector = null;

            if (groupDescriptors != null && groupDescriptors.Any())
            {
                foreach (var group in groupDescriptors.Reverse())
                {
                    if (selector == null)
                    {
                        if (group.Member == "OrderDetailID")
                        {
                            selector = OrderDetails => BuildInnerGroup(OrderDetails, o => o.OrderDetailID);
                        }
                        else if (group.Member == "AccountNumber")
                        {
                            selector = OrderDetails => BuildInnerGroup(OrderDetails, o => o.AccountNumber);
                        }
                        else if (group.Member == "OrderDate")
                        {
                            selector = OrderDetails => BuildInnerGroup(OrderDetails, o => o.OrderDate);
                        }
                        else if (group.Member == "ProductName")
                        {
                            selector = OrderDetails => BuildInnerGroup(OrderDetails, o => o.ProductName);
                        }
                        else if (group.Member == "OrderTrackingNumber")
                        {
                            selector = OrderDetails => BuildInnerGroup(OrderDetails, o => o.OrderTrackingNumber);
                        }
                    }
                    else
                    {
                        if (group.Member == "OrderDetailID")
                        {
                            selector = BuildGroup(o => o.OrderDetailID, selector);
                        }
                        else if (group.Member == "AccountNumber")
                        {
                            selector = BuildGroup(o => o.AccountNumber, selector);
                        }
                        else if (group.Member == "OrderDate")
                        {
                            selector = BuildGroup(o => o.OrderDate, selector);
                        }
                        else if (group.Member == "ProductName")
                        {
                            selector = BuildGroup(o => o.ProductName, selector);
                        }
                        else if (group.Member == "OrderTrackingNumber")
                        {
                            selector = BuildGroup(o => o.OrderTrackingNumber, selector);
                        }
                    }
                }
            }

            return selector;
        }

        private static Func<IEnumerable<OrderDetailViewModel>, IEnumerable<AggregateFunctionsGroup>> BuildGroup<T>(
            Expression<Func<OrderDetailViewModel, T>> groupSelector,
            Func<IEnumerable<OrderDetailViewModel>, IEnumerable<AggregateFunctionsGroup>> selectorBuilder)
        {
            var tempSelector = selectorBuilder;
            return g => g.GroupBy(groupSelector.Compile())
                         .Select(c => new AggregateFunctionsGroup
                         {
                             Key = c.Key,
                             HasSubgroups = true,
                             Member = groupSelector.MemberWithoutInstance(),
                             Items = tempSelector.Invoke(c).ToList(),
                             ItemCount = 33
                         });
        }

        private static IEnumerable<AggregateFunctionsGroup> BuildInnerGroup<T>( IEnumerable<OrderDetailViewModel> group, Expression<Func<OrderDetailViewModel, T>> groupSelector)
        {
            return group.GroupBy(groupSelector.Compile())
                    .Select(i => new AggregateFunctionsGroup
                    {
                        Key = i.Key,
                        Member = groupSelector.MemberWithoutInstance(),
                        Items = i.ToList(),
                        ItemCount = 33
                    });
        }

        //public static IQueryable<OrderDetailViewModel> ApplyOrdersSorting(this IQueryable<OrderDetailViewModel> data,
        //            IList<GroupDescriptor> groupDescriptors, IList<SortDescriptor> sortDescriptors)
        //{
        //    if (groupDescriptors != null && groupDescriptors.Any())
        //    {
        //        foreach (var groupDescriptor in groupDescriptors.Reverse())
        //        {
        //            data = AddSortExpression(data, groupDescriptor.SortDirection, groupDescriptor.Member);
        //        }
        //    }

        //    if (sortDescriptors != null && sortDescriptors.Any())
        //    {
        //        foreach (SortDescriptor sortDescriptor in sortDescriptors)
        //        {
        //            data = AddSortExpression(data, sortDescriptor.SortDirection, sortDescriptor.Member);
        //        }
        //    }

        //    return data;
        //}

        //private static IQueryable<OrderDetailViewModel> AddSortExpression(IQueryable<OrderDetailViewModel> data, ListSortDirection
        //            sortDirection, string memberName)
        //{
        //    if (sortDirection == ListSortDirection.Ascending)
        //    {
        //        switch (memberName)
        //        {
        //            case "OrderDetailID":
        //                data = data.OrderBy(orderDetails => orderDetails.OrderDetailID);
        //                break;
        //            case "AccountNumber":
        //                data = data.OrderBy(orderDetails => orderDetails.AccountNumber);
        //                break;
        //            case "OrderDate":
        //                data = data.OrderBy(orderDetails => orderDetails.OrderDate);
        //                break;
        //            case "ProductName":
        //                data = data.OrderBy(orderDetails => orderDetails.ProductName);
        //                break;
        //            case "OrderTrackingNumber":
        //                data = data.OrderBy(orderDetails => orderDetails.OrderTrackingNumber);
        //                break;
        //        }
        //    }
        //    else
        //    {
        //        switch (memberName)
        //        {
        //            case "OrderDetailID":
        //                data = data.OrderByDescending(order => order.OrderDetailID);
        //                break;
        //            case "AccountNumber":
        //                data = data.OrderByDescending(order => order.AccountNumber);
        //                break;
        //            case "OrderDate":
        //                data = data.OrderByDescending(order => order.OrderDate);
        //                break;
        //            case "ProductName":
        //                data = data.OrderByDescending(order => order.ProductName);
        //                break;
        //            case "OrderTrackingNumber":
        //                data = data.OrderByDescending(order => order.OrderTrackingNumber);
        //                break;
        //        }
        //    }
        //    return data;
        //}

        //public static IQueryable<OrderDetailViewModel> ApplyOrdersFiltering(this IQueryable<OrderDetailViewModel> data,
        //   IList<IFilterDescriptor> filterDescriptors)
        //{
        //    if (filterDescriptors.Any())
        //    {
        //        data = data.Where(ExpressionBuilder.Expression<OrderDetailViewModel>(filterDescriptors, false));
        //    }
        //    return data;
        //}
    }
}
