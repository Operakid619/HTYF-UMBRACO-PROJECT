using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace SeniorEventBooking.Migrations;


public class EnsureEventListDocTypeComposer : IComposer
{
    public void Compose(Umbraco.Cms.Core.DependencyInjection.IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<UmbracoApplicationStartedNotification, EnsureEventListDocTypeHandler>();
    }
}

public class EnsureEventListDocTypeHandler : INotificationHandler<UmbracoApplicationStartedNotification>
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IFileService _fileService;
    private readonly IShortStringHelper _shortStringHelper;

    public EnsureEventListDocTypeHandler(
        IContentTypeService contentTypeService,
        IFileService fileService,
        IShortStringHelper shortStringHelper)
    {
        _contentTypeService = contentTypeService;
        _fileService = fileService;
        _shortStringHelper = shortStringHelper;
    }

    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        const string templateAlias = "EventList";
        const string templateName = "Event List";
        const string docTypeAlias = "EventList";
        const string docTypeName = "Event List";

        // Ensure template exists
        var template = _fileService.GetTemplate(templateAlias);
        if (template == null)
        {
            template = new Template(_shortStringHelper, templateName, templateAlias)
            {
                Content = "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n@{ Layout = null; }\n<h1>Events</h1>\n@RenderBody()"
            };
            _fileService.SaveTemplate(template);
        }

        // Ensure doc type exists
        var eventList = _contentTypeService.Get(docTypeAlias);
        if (eventList == null)
        {
            var ct = new ContentType(_shortStringHelper, -1)
            {
                Alias = docTypeAlias,
                Name = docTypeName,
                Icon = "icon-calendar-alt",
                AllowedAsRoot = true
            };

            ct.AllowedTemplates = new[] { template };
            ct.SetDefaultTemplate(template);

            // If Event exists, try to allow it as child (best-effort)
            var eventCt = _contentTypeService.Get("Event");
            if (eventCt != null)
            {
                // Some Umbraco versions require ContentTypeSort with lazy id; best effort: save first then update allowed children
                _contentTypeService.Save(ct);
                eventList = _contentTypeService.Get(docTypeAlias);
                if (eventList != null)
                {
                    var allowed = eventList.AllowedContentTypes?.ToList() ?? new List<ContentTypeSort>();
                    allowed.Add(new ContentTypeSort(eventCt.Id, 0));
                    eventList.AllowedContentTypes = allowed.ToArray();
                    _contentTypeService.Save(eventList);
                }
                return;
            }

            _contentTypeService.Save(ct);
        }
        else
        {
            // Ensure template assigned
            if (!eventList.AllowedTemplates.Any(t => t.Alias == templateAlias))
            {
                var templates = eventList.AllowedTemplates.ToList();
                templates.Add(template);
                eventList.AllowedTemplates = templates.ToArray();
                eventList.SetDefaultTemplate(template);
                _contentTypeService.Save(eventList);
            }
        }
    }
}
