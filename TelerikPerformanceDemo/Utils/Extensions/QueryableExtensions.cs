using System;
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
                             Items = tempSelector.Invoke(c)//.ToList()
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
                        Items = i//.ToList()
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
