using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BeachApplication.TagHelpers;

[HtmlTargetElement(Attributes = "is-visible")]
public class IsVisibleTagHelper : TagHelper
{
    public bool IsVisible { get; set; } = true;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (!IsVisible)
        {
            output.SuppressOutput();
        }

        await base.ProcessAsync(context, output);
    }
}