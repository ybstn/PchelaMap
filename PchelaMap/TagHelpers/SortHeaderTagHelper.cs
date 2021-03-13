using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using PchelaMap.Models;

namespace PchelaMap.TagHelpers
{
    public class SortHeaderTagHelper : TagHelper
    {
        public SortSearchPagin.UserSortState current { get; set; }
        public SortSearchPagin.UserSortState property { get; set; }
        public string _action { get; set; }
        public bool Up { get; set; }

        private IUrlHelperFactory _urlHelperFactory;

        public SortHeaderTagHelper(IUrlHelperFactory helperFactory)
        {
            _urlHelperFactory = helperFactory;
        }
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
            output.TagName = "a";
            string url = urlHelper.Action(_action, new { sortOrder = property });
            output.Attributes.SetAttribute("href", url);
            if(property == current)
            {
                TagBuilder tag = new TagBuilder("i");
                tag.AddCssClass("sortIcon");
                if (Up)
                {
                    tag.AddCssClass("sortIconUp");
                }
                else
                {
                    tag.AddCssClass("sortIconDown");
                }
                output.PreContent.AppendHtml(tag);
            }
            //base.Process(context, output);
        }
    }
}
