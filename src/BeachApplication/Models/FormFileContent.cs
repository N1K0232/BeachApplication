namespace BeachApplication.Models;

public class FormFileContent
{
    public FormFileContent(IFormFile file, string description, bool overwrite)
    {
        File = file;
        Description = description;
        Overwrite = overwrite;
    }

    public IFormFile File { get; }

    public string Description { get; }

    public bool Overwrite { get; }

    public static async ValueTask<FormFileContent> BindAsync(HttpContext httpContext)
    {
        var form = await httpContext.Request.ReadFormAsync();
        if (form == null)
        {
            return null;
        }

        var file = form.Files.ElementAtOrDefault(0);
        if (file is null)
        {
            return null;
        }

        var description = form["Description"];
        var overwrite = Convert.ToBoolean(form["Overwrite"]);

        return new FormFileContent(file, description, overwrite);
    }
}