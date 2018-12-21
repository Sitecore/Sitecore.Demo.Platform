using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Security.Accounts;
using Sitecore.Workflows.Simple;

namespace Sitecore.HabitatHome.Foundation.Workflow.Actions
{
    public class SubmissionNotification
    {       
        /// <summary>
        /// Runs the processor.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void Process(WorkflowPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            ProcessorItem processorItem = args.ProcessorItem;
            if (processorItem == null)
            {
                return;
            }

            Item innerItem = processorItem.InnerItem;
                                                                  
            string fromAddress = this.GetText(innerItem, "from", args);   
            string mailServer = this.GetText(innerItem, "mail server", args);
            string subject = this.GetText(innerItem, "subject", args);
            string message = this.GetText(innerItem, "message", args);
            string approversRole = GetText(innerItem, "approvers role", args);                                                                

            string actionPath = innerItem.Paths.FullPath;
            Error.Assert(fromAddress.Length > 0, "The 'From' field is not specified in the mail action item: " + actionPath);
            Error.Assert(subject.Length > 0, "The 'Subject' field is not specified in the mail action item: " + actionPath);
            Error.Assert(mailServer.Length > 0, "The 'Mail server' field is not specified in the mail action item: " + actionPath);
                                                             
            message = ReplaceVariables(message, args);

            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            AddRecipientsToMail(mailMessage, approversRole);

            try
            {
                SmtpClient smtpClient = new SmtpClient(mailServer);
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {                      
                Log.Error($"Failed to send mail: {ex.Message}", ex, typeof(SubmissionNotification));
            }
        }

        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <param name="commandItem">The command item.</param>
        /// <param name="field">The field.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private string GetText(Item commandItem, string field, WorkflowPipelineArgs args)
        {
            string text = commandItem[field];
            if (text.Length > 0)
            {
                return this.ReplaceVariables(text, args);
            }
            return string.Empty;
        }

        /// <summary>
        /// Replaces the variables.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private string ReplaceVariables(string text, WorkflowPipelineArgs args)
        {
            Item contentItem = args.DataItem;

            var contentWorkflow = contentItem.Database.WorkflowProvider.GetWorkflow(contentItem);
            var contentHistory = contentWorkflow.GetHistory(contentItem);

            if (contentHistory.Length > 0)
            {
                var lastUser = contentHistory.Last().User;
                text = text.Replace("$submitter$", lastUser);
            }

            text = text.Replace("$itemPath$", args.DataItem.Paths.FullPath);
            text = text.Replace("$itemLanguage$", args.DataItem.Language.ToString());
            text = text.Replace("$itemVersion$", args.DataItem.Version.ToString());
            text = text.Replace("$hostname$", HttpContext.Current.Request.Url.Host);
            text = text.Replace ( "$comment$" , args.CommentFields[ "Comments" ] );


            return text;
        }

        public static MailMessage AddRecipientsToMail(MailMessage mailMessage, string roleName)
        {      
            if (Role.Exists(roleName))
            {
                Role role = Role.FromName(roleName);
                List<User> users = RolesInRolesManager.GetUsersInRole(role, true)
                    .Where(x => x.IsInRole(role))
                    .Where(user => !string.IsNullOrEmpty(user.Profile.Email))
                    .ToList();

                foreach (User user in users)
                {
                    mailMessage.To.Add(user.Profile.Email);
                }
            }
            else
            {
                Log.Error($"No Users with valid email addresses found in role {roleName}", typeof(SubmissionNotification));
            }

            return mailMessage;
        }
    }
}