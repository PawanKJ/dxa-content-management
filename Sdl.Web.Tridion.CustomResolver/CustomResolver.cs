﻿using System.Linq;
using Tridion.Collections;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Publishing;
using Tridion.ContentManager.Publishing.Resolving;

namespace Sdl.Web.Tridion
{
    /// <summary>
    /// DXA Custom Resolver to publish components added to a page with the 'Generate Data Presentation' template.
    /// 
    /// Usage:
    ///   1) Add this assembly to the GAC on the machine hosting the CME
    ///   2) Modify the \config\Tridion.ContentManager.config file and add the following to the resolving/mappings section:
    ///         <resolving>
    ///             <mappings>
    ///                 ...
    ///                 <add itemType="Tridion.ContentManager.CommunicationManagement.Page">
    ///                     <resolvers>
    ///                         ...
    ///                         <add type="Sdl.Web.Tridion.CustomResolver" assembly="Sdl.Web.Tridion.CustomResolver, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b9a30ebcbde732e4" />
    ///                     </resolvers>
    ///                 </add>
    ///             </mappings>
    ///         </resolving>
    /// </summary>
    public class CustomResolver : IResolver
    {
        public void Resolve(IdentifiableObject item, ResolveInstruction instruction, PublishContext context,
            ISet<ResolvedItem> resolvedItems)
        {
            if (instruction.Purpose != ResolvePurpose.Publish && instruction.Purpose != ResolvePurpose.RePublish)
                return;
            var resolvedItemList = resolvedItems.ToArray();
            resolvedItems.Clear();
            foreach (var resolvedItem in resolvedItemList)
            {
                resolvedItems.Add(resolvedItem);
                if (!(resolvedItem.Item is Page)) continue;
                var page = (Page)resolvedItem.Item;
                if (page.ComponentPresentations.Count <= 0) continue;
                var sourceItem = (RepositoryLocalObject)resolvedItem.Item;
                var contextPublication = (Publication)sourceItem.ContextRepository;
                var filter = new ComponentTemplatesFilter(item.Session)
                {
                    AllowedOnPage = false,
                    BaseColumns = ListBaseColumns.IdAndTitle
                };
                // hard-coded title of template, how else should we identify it?
                const string dataPresentationTemplateTitle = "Generate Data Presentation";
                var dataPresentationTemplate = contextPublication.GetComponentTemplates(filter).FirstOrDefault(
                    ct => ct.Title == dataPresentationTemplateTitle);
                // for each component lets resolve it with our data presentation template
                foreach (var cp in page.ComponentPresentations.Where(
                    cp =>
                        dataPresentationTemplate != null &&
                        dataPresentationTemplate.RelatedSchemas.Contains(cp.Component.Schema)))
                {
                    resolvedItems.Add(new ResolvedItem(cp.Component, dataPresentationTemplate));
                }
            }
        }
    }
}
