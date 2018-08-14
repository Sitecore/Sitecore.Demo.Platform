using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Workflows;
using Sitecore.Workflows.Simple;

namespace Sitecore.HabitatHome.Foundation.Workflow.Actions
{
    public class AdvanceRelatedItems
    {
        public void Process(WorkflowPipelineArgs args)
        {
            if (args.DataItem == null)
            {
                return;
            }

            Item dataItem = args.DataItem;

            IWorkflow dataItemWorkflow = dataItem.Database.WorkflowProvider.GetWorkflow(dataItem);

            if (dataItemWorkflow != null)
            {
                WorkflowState dataItemWorkflowState = dataItemWorkflow.GetState(dataItem);

                List<Item> relatedItems = new List<Item>();
                relatedItems.AddRange(GetItemsFromRenderingDatasources(dataItem));

                //todo: get related items from personalization rules data sources
                //todo: get related items from tests                                 

                List<ID> processedItems = new List<ID>();

                foreach (Item relatedItem in relatedItems)
                {
                    IWorkflow relatedItemWorkflow = relatedItem.Database.WorkflowProvider.GetWorkflow(relatedItem);

                    if (relatedItemWorkflow == null)
                    {
                        // start the same workflow for the related item as the main item
                        dataItemWorkflow.Start(relatedItem);
                        relatedItemWorkflow = dataItemWorkflow;
                    }

                    if (relatedItemWorkflow.WorkflowID == dataItemWorkflow.WorkflowID &&
                        !processedItems.Contains(relatedItem.ID))
                    {
                        // only process related items in the same workflow as the main item 
                        // if the related item already belongs to a different workflow, we'll ignore it
                        AdvanceWorkflow(args, dataItemWorkflowState, relatedItem, dataItemWorkflow);
                        processedItems.Add(relatedItem.ID);
                    }
                }
            }
        }

        private IEnumerable<Item> GetItemsFromRenderingDatasources(Item dataItem)
        {
            RenderingReference[] renderings = dataItem.Visualization.GetRenderings(Context.Device, true);
            foreach (RenderingReference rendering in renderings)
            {
                Item item = Sitecore.Context.ContentDatabase.GetItem(rendering.Settings.DataSource);
                if (item != null && item.Paths.FullPath.StartsWith("/sitecore/content/", StringComparison.OrdinalIgnoreCase))
                {
                    yield return item;
                }
            }
        }

        private void AdvanceWorkflow(WorkflowPipelineArgs args, WorkflowState dataItemWorkflowState, Item relatedItem, IWorkflow relatedItemWorkflow)
        {
            List<WorkflowCommand> commands = relatedItemWorkflow.GetCommands(relatedItem).ToList();

            if (commands.Any())
            {
                foreach (WorkflowCommand command in commands)
                {
                    Item commandItem = Sitecore.Context.ContentDatabase.GetItem(command.CommandID);

                    if (commandItem != null && commandItem.Fields["Next state"].Value == dataItemWorkflowState.StateID)
                    {
                        try
                        {
                            relatedItemWorkflow.Execute(command.CommandID, relatedItem, args.CommentFields, false, args.Parameters);

                            Log.Info("Item: " + relatedItem.DisplayName + " was successfully advanced to workflow", this);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(relatedItem.DisplayName + " cannot be advanced to workflow", ex, this);
                        }
                    }
                }
            }
        }
    }
}