﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.Infrastructure;
using Kendo.Mvc.UI;
using TelerikPerformanceDemo.Models;

namespace TelerikPerformanceDemo.Utils.Extensions
{
    public static class QueryableExtensions
    {
        public static DataSourceResult ToCustomDataSourceResult(this IQueryable<OrderDetailViewModel> source, DataSourceRequest request)
        {
            source = source.ApplyOrdersFiltering(request.Filters);

            var total = source.Count();

            source = source.ApplyOrdersSorting(request.Groups, request.Sorts);

            if (!request.Sorts.Any())
            {
                source = source.OrderBy(o => o.OrderDetailID);
            }

            var selector = source.VerifyOrdersGrouping(request.Groups);

            IEnumerable data;
            if (selector != null)
            {
                var skip = request.Page > 0 ? (request.Page - 1) * request.PageSize : 0;
                data = selector.Invoke(source).Skip(skip).Take(request.PageSize).ToList();
            }
            else
            {
                data = source.ApplyOrdersPaging(request.Page, request.PageSize).ToList();
            }

            var result = new DataSourceResult()
            {
                Data = data,
                Total = total
            };

            return result;
        }

        public static DataSourceResult ToCustomDataSourceResultalt(this IQueryable<OrderDetailViewModel> source, DataSourceRequest request)
        {
            // FILTRE
            source = source.ApplyOrdersFiltering(request.Filters);

            // TRIER
            source = source.ApplyOrdersSorting(request.Groups, request.Sorts);
           
            // TRIER DEFAUT
            if (!request.Sorts.Any())
            {
                source = source.OrderBy(o => o.OrderDetailID);
            }

            // GROUPE / AGGREGATE
            DataSourceResult result = null;
            var methodeAlt = false;

            if (!methodeAlt)
            {
                result = AppliquerGroupePagination(source, request);
            }
            else
            {
                result = AppliquerGroupePaginationAlternative(source, request);
            }
           

            return result;
        }

        private static DataSourceResult AppliquerGroupePagination(IQueryable<OrderDetailViewModel> source, DataSourceRequest request)
        {
            var selector = source.VerifyOrdersGrouping(request.Groups);
            var total = source.Count();
            IEnumerable data;
            if (selector != null)
            {
                var skip = request.Page > 0 ? (request.Page - 1) * request.PageSize : 0;
                data = selector.Invoke(source).Skip(skip).Take(request.PageSize).ToList();
            }
            else
            {
                data = source.ApplyOrdersPaging(request.Page, request.PageSize).ToList();
            }

            var result = new DataSourceResult()
            {
                Data = data,
                Total = total
            };
            return result;
        }

        private static DataSourceResult AppliquerGroupePaginationAlternative(IQueryable<OrderDetailViewModel> source, DataSourceRequest request)
        {
            var noPageCourante = (request.Page - 1);
            var customGrouping = ConvertirGroupeEnOrderby(source, request, noPageCourante);

            var skip = request.Page > 0 ? (noPageCourante) * request.PageSize : 0;
            var result = new DataSourceResult()
            {
                Data = customGrouping.Data.AsQueryable().Skip(skip).Take(request.PageSize),
                Total = customGrouping.Total
            };

            return result;
        }


        private static DataSourceResult ConvertirGroupeEnOrderby(IQueryable<OrderDetailViewModel> source, DataSourceRequest request, int noPage)
        {
            var grouping = request.Groups;
            //Add ordering for the grouping
            var tempSort = grouping.Select(g => new SortDescriptor()
            {
                Member = g.Member,
                SortDirection = g.SortDirection
            }).ToList();

            tempSort.Reverse();
            tempSort.ForEach(x => request.Sorts.Insert(0, x));


            //var t = source.GroupBy

            //Remove the grouping before exeecuting the query against NHibernate
            request.Groups = null;
            //Execute the query and return results from the database.
            //These results will be paged, filtered and ordered.
            var result = source.ToDataSourceResult(request);
            var total = result.Total;
            request.Groups = grouping;
            request.Page = noPage;
            result = result.Data.ToDataSourceResult(request);
            result.Total = total;


            return result;
        }

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

        private static IEnumerable<AggregateFunctionsGroup> BuildInnerGroup<T>(
            IEnumerable<OrderDetailViewModel> group,
            Expression<Func<OrderDetailViewModel, T>> groupSelector)
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

        public static IQueryable<OrderDetailViewModel> ApplyOrdersSorting(this IQueryable<OrderDetailViewModel> data,
                    IList<GroupDescriptor> groupDescriptors, IList<SortDescriptor> sortDescriptors)
        {
            if (groupDescriptors != null && groupDescriptors.Any())
            {
                foreach (var groupDescriptor in groupDescriptors.Reverse())
                {
                    data = AddSortExpression(data, groupDescriptor.SortDirection, groupDescriptor.Member);
                }
            }

            if (sortDescriptors != null && sortDescriptors.Any())
            {
                foreach (SortDescriptor sortDescriptor in sortDescriptors)
                {
                    data = AddSortExpression(data, sortDescriptor.SortDirection, sortDescriptor.Member);
                }
            }

            return data;
        }

        private static IQueryable<OrderDetailViewModel> AddSortExpression(IQueryable<OrderDetailViewModel> data, ListSortDirection
                    sortDirection, string memberName)
        {
            if (sortDirection == ListSortDirection.Ascending)
            {
                switch (memberName)
                {
                    case "OrderDetailID":
                        data = data.OrderBy(orderDetails => orderDetails.OrderDetailID);
                        break;
                    case "AccountNumber":
                        data = data.OrderBy(orderDetails => orderDetails.AccountNumber);
                        break;
                    case "OrderDate":
                        data = data.OrderBy(orderDetails => orderDetails.OrderDate);
                        break;
                    case "ProductName":
                        data = data.OrderBy(orderDetails => orderDetails.ProductName);
                        break;
                    case "OrderTrackingNumber":
                        data = data.OrderBy(orderDetails => orderDetails.OrderTrackingNumber);
                        break;
                }
            }
            else
            {
                switch (memberName)
                {
                    case "OrderDetailID":
                        data = data.OrderByDescending(order => order.OrderDetailID);
                        break;
                    case "AccountNumber":
                        data = data.OrderByDescending(order => order.AccountNumber);
                        break;
                    case "OrderDate":
                        data = data.OrderByDescending(order => order.OrderDate);
                        break;
                    case "ProductName":
                        data = data.OrderByDescending(order => order.ProductName);
                        break;
                    case "OrderTrackingNumber":
                        data = data.OrderByDescending(order => order.OrderTrackingNumber);
                        break;
                }
            }
            return data;
        }

        public static IQueryable<OrderDetailViewModel> ApplyOrdersFiltering(this IQueryable<OrderDetailViewModel> data,
           IList<IFilterDescriptor> filterDescriptors)
        {
            if (filterDescriptors.Any())
            {
                data = data.Where(ExpressionBuilder.Expression<OrderDetailViewModel>(filterDescriptors, false));
            }
            return data;
        }
    }
}
