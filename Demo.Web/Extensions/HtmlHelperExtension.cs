﻿using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;

namespace Demo.Web.Extensions
{
    public static class HtmlHelperExtension
    {
        public static IHtmlContent RequireLabel(this IHtmlHelper htmlHelper)
        {
            return new HtmlString("<span class='red'>(*)</span>");
        }

        public static IHtmlContent Img(string dataSrc, string alt = "", string classCss = "", string style = "")
        {
            return new HtmlString($"<img src=\"/images/loading.png\" data-src=\"{dataSrc}\" alt=\"{alt}\" class=\"lazyload {classCss}\" style=\"{style}\" />");
        }

        public static bool IsAjaxRequest(this HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}
