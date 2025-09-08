using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace ToSic.Eav.WebApi.Sys.Admin.Odata;

internal class CoreSystemQueryOptionsParser
{
    ///// <summary>
    ///// Detect only OData system options that start with $
    ///// "$select","$filter","$orderby","$top","$skip","$count",
    ///// "$expand","$search","$apply","$compute","$format","$skiptoken","$deltatoken"
    ///// </summary>
    ///// <param name="uri"></param>
    ///// <returns></returns>
    //public static bool HasDollarSystemOptions(Uri uri)
    //{
    //    // matches only $… keys; also handles %24-encoded $
    //    return Regex.IsMatch(
    //        uri.Query ?? "",
    //        @"(?:^|\?|&)(?:\$|%24)(select|filter|orderby|top|skip|count|expand|search|apply|compute|format|skiptoken|deltatoken)=",
    //        RegexOptions.IgnoreCase);
    //    // - Anchors to the start of the query or a separator ?/ &.
    //    // - Requires the name to begin with $ (or % 24).
    //    // - Ensures it’s a parameter name(followed by =), not appearing in a value.
    //}

    // Example usage
    public static ODataUri Parse(Uri? fullRequest)
    {
        //var fullRequest = uri ?? new Uri($"https://dummy/app/auto/data/BlogPost/?$select={oData}");
        var serviceRoot = ExtractServiceRoot(fullRequest);   // => http://…/odata1/api/2sxc/app/auto/data/
        var model = BuildModel();

        var parser = new ODataUriParser(model, serviceRoot, fullRequest)
        {
            Resolver = new ODataUriResolver { EnableCaseInsensitive = true }, // "title" -> Title
            EnableNoDollarQueryOptions = false,
            UrlKeyDelimiter = ODataUrlKeyDelimiter.Slash,

        };
        parser.Settings.MaximumExpansionDepth = 0;   // no $expand in v1
        parser.Settings.MaximumExpansionCount = 0;

        // One-shot parse
        return parser.ParseUri();
    }

    // Build EDM for BlogApp from your screenshots (minimal properties + relations)
    private static IEdmModel BuildModel()
    {
        var model = new EdmModel();

        // Types (open so extra EAV fields won’t break parsing)
        var blogPost = new EdmEntityType("BlogApp", "BlogPost", baseType: null, isAbstract: false, isOpen: true);
        var blogId = blogPost.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32);
        blogPost.AddKeys(blogId);
        blogPost.AddStructuralProperty("Title", EdmPrimitiveTypeKind.String);
        blogPost.AddStructuralProperty("Content", EdmPrimitiveTypeKind.String);
        blogPost.AddStructuralProperty("UrlKey", EdmPrimitiveTypeKind.String);
        blogPost.AddStructuralProperty("PublicationMoment", EdmPrimitiveTypeKind.DateTimeOffset);
        blogPost.AddStructuralProperty("ShowOnStartPage", EdmPrimitiveTypeKind.Boolean);
        model.AddElement(blogPost);

        var author = new EdmEntityType("BlogApp", "Author", baseType: null, isAbstract: false, isOpen: true);
        var authorId = author.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32);
        author.AddKeys(authorId);
        author.AddStructuralProperty("FullName", EdmPrimitiveTypeKind.String);
        author.AddStructuralProperty("Key", EdmPrimitiveTypeKind.String);
        author.AddStructuralProperty("Image", EdmPrimitiveTypeKind.String);
        author.AddStructuralProperty("ShortBio", EdmPrimitiveTypeKind.String);
        model.AddElement(author);

        var category = new EdmEntityType("BlogApp", "Category", baseType: null, isAbstract: false, isOpen: true);
        var catId = category.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32);
        category.AddKeys(catId);
        category.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
        category.AddStructuralProperty("Key", EdmPrimitiveTypeKind.String);
        category.AddStructuralProperty("Description", EdmPrimitiveTypeKind.String);
        model.AddElement(category);

        // Navigations on BlogPost
        var navAuthor = blogPost.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
        {
            Name = "Author",
            Target = author,
            TargetMultiplicity = EdmMultiplicity.ZeroOrOne
        });
        var navCategories = blogPost.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
        {
            Name = "Categories",
            Target = category,
            TargetMultiplicity = EdmMultiplicity.Many
        });

        // Container + Sets (use singular names to match your path "…/data/BlogPost/")
        var container = new EdmEntityContainer("BlogApp", "Container");
        var blogPosts = container.AddEntitySet("BlogPost", blogPost);
        var authors = container.AddEntitySet("Author", author);
        var categories = container.AddEntitySet("Category", category);

        // Bind nav targets
        blogPosts.AddNavigationTarget(navAuthor, authors);
        blogPosts.AddNavigationTarget(navCategories, categories);

        model.AddElement(container);
        return model;
    }

    // Extract service root ending with ".../app/auto/data/"
    static Uri ExtractServiceRoot(Uri full)
    {
        const string marker = "/app/auto/data/";
        var s = full.AbsoluteUri;
        var i = s.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (i < 0) throw new InvalidOperationException("Cannot find service-root marker '/app/auto/data/'.");
        return new Uri(s.Substring(0, i + marker.Length));
    }

    public static IReadOnlyList<string> GetSelectedProperties(SelectExpandClause sec)
    {
        var list = new List<string>();
        if (sec == null) return list;
        foreach (var item in sec.SelectedItems)
        {
            if (item is PathSelectItem p)
            {
                var seg = p.SelectedPath?.LastSegment as PropertySegment;
                if (seg != null) list.Add(seg.Property.Name);
            }
            else if (item is WildcardSelectItem) list.Add("*");
        }
        return list;
    }
}
