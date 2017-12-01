namespace Sitecore.Foundation.SitecoreExtensions.Extensions
{
    using Sitecore.Foundation.SitecoreExtensions.Attributes;
    using Sitecore.Mvc;
    using System;
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Generates a hidden form field for use with form validation
        /// Required for the <see cref="ValidateRenderingIdAttribute">ValidateRenderingIdAttribute</see> to work
        /// </summary>
        /// <param name="htmlHelper">Html Helper class</param>
        /// <returns>Hidden form field with unique ID</returns>
        public static MvcHtmlString AddUniqueFormId(this HtmlHelper htmlHelper)
        {
            var uid = htmlHelper.Sitecore().CurrentRendering?.UniqueId;
            return !uid.HasValue ? null : htmlHelper.Hidden(ValidateRenderingIdAttribute.FormUniqueid, uid.Value);
        }

        public static MvcHtmlString ValidationErrorFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string error)
        {
            return htmlHelper.HasError(ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData), ExpressionHelper.GetExpressionText(expression)) ? new MvcHtmlString(error) : null;
        }

        public static bool HasError(this HtmlHelper htmlHelper, ModelMetadata modelMetadata, string expression)
        {
            var modelName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(expression);
            var formContext = htmlHelper.ViewContext.FormContext;
            if (formContext == null)
            {
                return false;
            }

            if (!htmlHelper.ViewData.ModelState.ContainsKey(modelName))
            {
                return false;
            }

            var modelState = htmlHelper.ViewData.ModelState[modelName];
            var modelErrors = modelState?.Errors;
            return modelErrors?.Count > 0;
        }
    }
}