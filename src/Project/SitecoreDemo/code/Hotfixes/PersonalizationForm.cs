namespace Sitecore.Demo.Platform.Website.Hotfixes
{
	using Sitecore.Annotations;
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Web;
	using System.Web.UI;
	using System.Xml.Linq;
	using Sitecore.Configuration;
	using Sitecore.Data;
	using Sitecore.Data.Items;
	using Sitecore.Data.Managers;
	using Sitecore.Diagnostics;
	using Sitecore.Extensions.XElementExtensions;
	using Sitecore.Globalization;
	using Sitecore.Layouts;
	using Sitecore.Pipelines;
	using Sitecore.Pipelines.GetPlaceholderRenderings;
	using Sitecore.Pipelines.GetRenderingDatasource;
	using Sitecore.Pipelines.RenderRulePlaceholder;
	using Sitecore.Resources;
	using Sitecore.Rules;
	using Sitecore.Shell.Applications.Dialogs.Personalize;
	using Sitecore.Shell.Applications.Dialogs.RulesEditor;
	using Sitecore.Shell.Applications.Rules;
	using Sitecore.Shell.Controls;
	using Sitecore.Web;
	using Sitecore.Web.UI;
	using Sitecore.Web.UI.HtmlControls;
	using Sitecore.Web.UI.Pages;
	using Sitecore.Web.UI.Sheer;
	using StringExtensions;

	// DEMO TEAM CUSTOMIZATION: #4770 #7048 Do not set "Track Personalization Effect" by default
	public class PersonalizationForm : DialogForm
	{
		#region Constants and Fields
		public const string AfterActionPlaceholderName = "afterAction";

		protected readonly string ConditionDescriptionDefault;

		protected static readonly string DefaultConditionId = Sitecore.Analytics.AnalyticsIds.DefaultPersonalizationRuleCondition.ToString();

		protected readonly string ConditionNameDefault;

		protected Checkbox ComponentPersonalization;

		protected Checkbox PersonalizationTracking;

		protected Scrollbox RulesContainer;

		private string HideRenderingActionId = RuleIds.HideRenderingActionId.ToString();

		private string SetDatasourceActionId = RuleIds.SetDatasourceActionId.ToString();

		private string SetRenderingActionId = RuleIds.SetRenderingActionId.ToString();

		private readonly string newConditionName = Translate.Text(Texts.Newcondition);

		private readonly XElement defaultCondition;

		#endregion

		#region Properties

		[Sitecore.Annotations.CanBeNull]
		public Item ContextItem
		{
			get
			{
				ItemUri uri = ItemUri.Parse(this.ContextItemUri);
				if (uri != null)
				{
					return Database.GetItem(uri);
				}

				return null;
			}
		}

		[Sitecore.Annotations.CanBeNull]
		public string ContextItemUri
		{
			get
			{
				return this.ServerProperties["ContextItemUri"] as string;
			}

			set
			{
				this.ServerProperties["ContextItemUri"] = value;
			}
		}

		[Sitecore.Annotations.NotNull]
		public string DeviceId
		{
			get
			{
				var value = this.ServerProperties["deviceId"] as string;
				return Assert.ResultNotNull(value);
			}

			set
			{
				Assert.IsNotNullOrEmpty(value, "value");

				this.ServerProperties["deviceId"] = value;
			}
		}

		[Sitecore.Annotations.NotNull]
		public string Layout
		{
			get
			{
				string key = this.SessionHandle;
				return Assert.ResultNotNull(WebUtil.GetSessionString(key));
			}
		}

		[Sitecore.Annotations.NotNull]
		public LayoutDefinition LayoutDefition
		{
			get
			{
				return LayoutDefinition.Parse(this.Layout);
			}
		}

		[Sitecore.Annotations.NotNull]
		public string ReferenceId
		{
			get
			{
				var value = this.ServerProperties["referenceId"] as string;
				return Assert.ResultNotNull(value);
			}

			set
			{
				Assert.IsNotNullOrEmpty(value, "value");

				this.ServerProperties["referenceId"] = value;
			}
		}

		[Sitecore.Annotations.NotNull]
		public RenderingDefinition RenderingDefition
		{
			get
			{
				return
				  Assert.ResultNotNull(this.LayoutDefition.GetDevice(this.DeviceId).GetRenderingByUniqueId(this.ReferenceId));
			}
		}

		[Sitecore.Annotations.NotNull]
		public XElement RulesSet
		{
			get
			{
				var value = this.ServerProperties["ruleSet"] as string;
				if (!string.IsNullOrEmpty(value))
				{
					return XElement.Parse(value);
				}

				return new XElement("ruleset", this.defaultCondition);
			}

			set
			{
				Assert.ArgumentNotNull(value, "value");

				this.ServerProperties["ruleSet"] = value.ToString();
			}
		}

		[Sitecore.Annotations.NotNull]
		public string SessionHandle
		{
			get
			{
				var value = this.ServerProperties["SessionHandle"] as string;
				return Assert.ResultNotNull(value);
			}

			set
			{
				Assert.IsNotNullOrEmpty(value, "session handle");
				this.ServerProperties["SessionHandle"] = value;
			}
		}

		public bool IsItemSupportPersonalizationTracking
		{
			get
			{
				Item contextItem = ContextItem;

				if (contextItem == null)
				{
					return false;
				}

				return !TemplateManager.IsTemplatePart(contextItem)
	&& !TemplateManager.IsStandardValuesHolder(contextItem);
			}
		}

		#endregion

		#region Methods

		public PersonalizationForm()
		{
			ConditionDescriptionDefault = Translate.Text(Texts.Ifnoneoftheotherconditionsaretruethedefault);
			ConditionNameDefault = Translate.Text(Texts.DEFAULT);
			this.newConditionName = Translate.Text(Texts.Newcondition);

			defaultCondition =
			  XElement.Parse(
				string.Format(
				  "<rule uid=\"{0}\" name=\"{1}\"><conditions><condition id=\"{2}\" uid=\"{3}\" /></conditions><actions /></rule>",
				  DefaultConditionId,
				  ConditionNameDefault,
				  RuleIds.TrueConditionId,
				  ID.NewID.ToShortID()));
		}

		protected void ComponentPersonalizationClick()
		{
			if (!this.ComponentPersonalization.Checked)
			{
				if (this.PersonalizeComponentActionExists())
				{
					var parameters = new NameValueCollection();
					Context.ClientPage.Start(this, "ShowConfirm", parameters);
					return;
				}
			}

			SheerResponse.Eval("scTogglePersonalizeComponentSection()");
		}

		protected void DeleteRuleClick([NotNull] string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException(nameof(id));
			}

			string uId = ID.Decode(id).ToString();

			XElement ruleSet = RulesSet;

			XElement ruleToDelete = ruleSet.Elements("rule")
				.Where(node => node.GetAttributeValue("uid") == uId)
				.FirstOrDefault();

			if (ruleToDelete != null)
			{
				ruleToDelete.Remove();
				RulesSet = ruleSet;

				SheerResponse.Remove(id + "data");
				SheerResponse.Remove(id);

				bool shouldOverrideState = ruleSet.Elements().Count() > 1;

				SetPersonalizationTrackingCheckbox(
					shouldSkipSupportedItemCheck: true,
					checkboxState: shouldOverrideState
						? PersonalizationTracking.Checked
						: null as bool?);
			}
		}

		protected void EditCondition([Sitecore.Annotations.NotNull] ClientPipelineArgs args)
		{
			Assert.ArgumentNotNull(args, "args");

			if (string.IsNullOrEmpty(args.Parameters["id"]))
			{
				SheerResponse.Alert(Texts.Pleaseselectarule);
				return;
			}

			string conditionId = ID.Decode(args.Parameters["id"]).ToString();

			if (!args.IsPostBack)
			{
				var options = new RulesEditorOptions
				{
					IncludeCommon = true,
					RulesPath = "/sitecore/system/settings/Rules/Conditional Renderings",
					AllowMultiple = false
				};

				XElement condition =
				  this.RulesSet.Elements("rule").Where(node => node.GetAttributeValue("uid") == conditionId).FirstOrDefault();

				if (condition != null)
				{
					options.Value = "<ruleset>" + condition + "</ruleset>";
				}

				options.HideActions = true;
				SheerResponse.ShowModalDialog(options.ToUrlString().ToString(), "580px", "712px", string.Empty, true);
				args.WaitForPostBack();
			}
			else if (args.HasResult)
			{
				string result = args.Result;
				XElement ruleNode = XElement.Parse(result).Element("rule");
				XElement rulesSet = this.RulesSet;
				if (ruleNode != null)
				{
					XElement originalRule =
					  rulesSet.Elements("rule").Where(node => node.GetAttributeValue("uid") == conditionId).
						FirstOrDefault();
					if (originalRule != null)
					{
						originalRule.ReplaceWith(ruleNode);
						this.RulesSet = rulesSet;
						SheerResponse.SetInnerHtml(args.Parameters["id"] + "_rule", GetRuleConditionsHtml(ruleNode));
					}
				}
			}
		}

		protected void EditConditionClick([Sitecore.Annotations.NotNull] string id)
		{
			Assert.ArgumentNotNull(id, "id");

			var parameters = new NameValueCollection();
			parameters["id"] = id;
			Context.ClientPage.Start(this, "EditCondition", parameters);
		}

		protected void MoveConditionAfter([Sitecore.Annotations.NotNull] string id, [Sitecore.Annotations.NotNull] string targetId)
		{
			Assert.ArgumentNotNull(id, "id");
			Assert.ArgumentNotNull(targetId, "targetId");

			XElement ruleSet = this.RulesSet;
			XElement rule = GetRuleById(ruleSet, id);
			XElement targetRule = GetRuleById(ruleSet, targetId);
			if (rule != null && targetRule != null)
			{
				rule.Remove();
				targetRule.AddAfterSelf(rule);
				this.RulesSet = ruleSet;
			}
		}

		protected void MoveConditionBefore([Sitecore.Annotations.NotNull] string id, [Sitecore.Annotations.NotNull] string targetId)
		{
			Assert.ArgumentNotNull(id, "id");
			Assert.ArgumentNotNull(targetId, "targetId");

			XElement ruleSet = this.RulesSet;
			XElement rule = GetRuleById(ruleSet, id);
			XElement targetRule = GetRuleById(ruleSet, targetId);
			if (rule != null && targetRule != null)
			{
				rule.Remove();
				targetRule.AddBeforeSelf(rule);
				this.RulesSet = ruleSet;
			}
		}

		protected void NewConditionClick()
		{
			var newRule = new XElement("rule");
			newRule.SetAttributeValue("name", this.newConditionName);

			ID id = ID.NewID;
			newRule.SetAttributeValue("uid", id);

			XElement rules = this.RulesSet;
			rules.AddFirst(newRule);

			RulesSet = rules;

			string html = this.GetRuleSectionHtml(newRule);
			SheerResponse.Insert("non-default-container", "afterBegin", html);

			SheerResponse.Eval("Sitecore.CollapsiblePanel.addNew(\"" + id.ToShortID() + "\")");

			bool isNewlyAddedPersonalizationRule = rules.Elements().Count() == 2;

			bool isNewlyPersonalizationRuleAddedOnEnabledCheckbox = isNewlyAddedPersonalizationRule && !PersonalizationTracking.Disabled;

			bool checkboxState = isNewlyPersonalizationRuleAddedOnEnabledCheckbox && PersonalizationTracking.Checked;

			SetPersonalizationTrackingCheckbox(checkboxState: checkboxState);
		}

		protected override void OnLoad([Sitecore.Annotations.NotNull] EventArgs e)
		{
			Assert.ArgumentNotNull(e, "e");

			base.OnLoad(e);
			if (Context.ClientPage.IsEvent)
			{
				SheerResponse.Eval("Sitecore.CollapsiblePanel.collapseMenus()");
				return;
			}

			PersonalizeOptions options = PersonalizeOptions.Parse();
			this.DeviceId = options.DeviceId;
			this.ReferenceId = options.RenderingUniqueId;
			this.SessionHandle = options.SessionHandle;
			this.ContextItemUri = options.ContextItemUri;

			RenderingDefinition rendering = this.RenderingDefition;
			XElement rules = rendering.Rules;
			if (rules != null)
			{
				this.RulesSet = rules;
			}

			if (this.PersonalizeComponentActionExists())
			{
				this.ComponentPersonalization.Checked = true;
			}

			this.RenderRules();

			SetPersonalizationTrackingCheckbox();
		}

		protected override void OnOK([Sitecore.Annotations.NotNull] object sender, [Sitecore.Annotations.NotNull] EventArgs args)
		{
			Assert.ArgumentNotNull(sender, "sender");
			Assert.ArgumentNotNull(args, "args");

			XElement rules = this.RulesSet;

			if (PersonalizationTrackingCheckboxEnabled())
			{
				rules.SetAttributeValue("pet", PersonalizationTracking.Checked);
			}

			SheerResponse.SetDialogValue(rules.ToString());

			base.OnOK(sender, args);
		}

		[HandleMessage("rule:rename")]
		protected void RenameRuleClick([Sitecore.Annotations.NotNull] Message message)
		{
			Assert.ArgumentNotNull(message, "message");
			var id = message.Arguments["ruleId"];
			var newName = message.Arguments["name"];

			Assert.IsNotNull(id, "id");
			if (!string.IsNullOrEmpty(newName))
			{
				XElement ruleSet = this.RulesSet;
				XElement rule = GetRuleById(ruleSet, id);
				if (rule != null)
				{
					rule.SetAttributeValue("name", newName);
					this.RulesSet = ruleSet;
				}
			}
		}

		protected void ResetDatasource([Sitecore.Annotations.NotNull] string id)
		{
			Assert.ArgumentNotNull(id, "id");

			if (!this.IsComponentDisplayed(id))
			{
				return;
			}

			XElement ruleSet = this.RulesSet;
			XElement rule = GetRuleById(ruleSet, id);
			if (rule != null)
			{
				RemoveAction(rule, SetDatasourceActionId);
				this.RulesSet = ruleSet;
				var output = new HtmlTextWriter(new StringWriter());
				this.RenderSetDatasourceAction(rule, output);
				SheerResponse.SetInnerHtml(id + "_setdatasource", output.InnerWriter.ToString().Replace("{ID}", id));
			}
		}


		protected void ResetRendering([Sitecore.Annotations.NotNull] string id)
		{
			Assert.ArgumentNotNull(id, "id");

			if (!this.IsComponentDisplayed(id))
			{
				return;
			}

			XElement ruleSet = this.RulesSet;
			XElement rule = GetRuleById(ruleSet, id);
			if (rule != null)
			{
				RemoveAction(rule, SetRenderingActionId);
				this.RulesSet = ruleSet;
				var output = new HtmlTextWriter(new StringWriter());
				this.RenderSetRenderingAction(rule, output);
				SheerResponse.SetInnerHtml(id + "_setrendering", output.InnerWriter.ToString().Replace("{ID}", id));
			}
		}

		protected void SetDatasource([Sitecore.Annotations.NotNull] ClientPipelineArgs args)
		{
			Assert.ArgumentNotNull(args, "args");
			string ruleId = args.Parameters["id"];
			XElement ruleSet = this.RulesSet;
			XElement rule = GetRuleById(ruleSet, ruleId);
			Assert.IsNotNull(rule, "rule");

			if (!args.IsPostBack)
			{
				XElement setReneringAction = GetActionById(rule, SetRenderingActionId);
				Item renderingItem = null;
				if (setReneringAction != null && !string.IsNullOrEmpty(setReneringAction.GetAttributeValue("RenderingItem")))
				{
					renderingItem = Client.ContentDatabase.GetItem(setReneringAction.GetAttributeValue("RenderingItem"));
				}
				else if (!string.IsNullOrEmpty(this.RenderingDefition.ItemID))
				{
					renderingItem = Client.ContentDatabase.GetItem(this.RenderingDefition.ItemID);
				}

				if (renderingItem == null)
				{
					SheerResponse.Alert(Texts.ITEM_NOT_FOUND);
					return;
				}

				Item contextItem = this.ContextItem;
				var pipelineArgs = new GetRenderingDatasourceArgs(renderingItem)
				{
					FallbackDatasourceRoots = new List<Item> { Client.ContentDatabase.GetRootItem() },
					ContentLanguage = contextItem != null ? contextItem.Language : null,
					ContextItemPath = contextItem != null ? contextItem.Paths.LongID : string.Empty,
					ShowDialogIfDatasourceSetOnRenderingItem = true
				};

				XElement setDatasourceAction = GetActionById(rule, SetDatasourceActionId);
				if (setDatasourceAction != null && !string.IsNullOrEmpty(setDatasourceAction.GetAttributeValue("DataSource")))
				{
					pipelineArgs.CurrentDatasource = setDatasourceAction.GetAttributeValue("DataSource");
				}
				else
				{
					pipelineArgs.CurrentDatasource = this.RenderingDefition.Datasource;
				}

				if (string.IsNullOrEmpty(pipelineArgs.CurrentDatasource))
					pipelineArgs.CurrentDatasource = contextItem.ID.ToString();

				CorePipeline.Run("getRenderingDatasource", pipelineArgs);
				if (string.IsNullOrEmpty(pipelineArgs.DialogUrl))
				{
					SheerResponse.Alert(Texts.AnErrorOcurred);
					return;
				}

				SheerResponse.ShowModalDialog(pipelineArgs.DialogUrl, "960px", "660px", string.Empty, true);
				args.WaitForPostBack();
			}
			else if (args.HasResult)
			{
				XElement setDatasourceAction = GetActionById(rule, SetDatasourceActionId) ??
				  AddAction(rule, SetDatasourceActionId);

				setDatasourceAction.SetAttributeValue("DataSource", args.Result);
				this.RulesSet = ruleSet;
				var output = new HtmlTextWriter(new StringWriter());
				this.RenderSetDatasourceAction(rule, output);

				SheerResponse.SetInnerHtml(ruleId + "_setdatasource", output.InnerWriter.ToString().Replace("{ID}", ruleId));
			}
		}

		protected void SetDatasourceClick([Sitecore.Annotations.NotNull] string id)
		{
			Assert.ArgumentNotNull(id, "id");

			if (!this.IsComponentDisplayed(id))
			{
				return;
			}

			var parameters = new NameValueCollection();
			parameters["id"] = id;
			Context.ClientPage.Start(this, "SetDatasource", parameters);
		}

		protected void SetRendering([Sitecore.Annotations.NotNull] ClientPipelineArgs args)
		{
			Assert.ArgumentNotNull(args, "args");

			if (!args.IsPostBack)
			{
				string placeholder = this.RenderingDefition.Placeholder;
				Assert.IsNotNull(placeholder, "placeholder");
				string layout = this.Layout;

				var placeholderRenderingArgs = new GetPlaceholderRenderingsArgs(placeholder, layout, Client.ContentDatabase, ID.Parse(this.DeviceId));
				placeholderRenderingArgs.OmitNonEditableRenderings = true;
				placeholderRenderingArgs.Options.ShowOpenProperties = false;

				CorePipeline.Run("getPlaceholderRenderings", placeholderRenderingArgs);
				string url = placeholderRenderingArgs.DialogURL;
				if (string.IsNullOrEmpty(url))
				{
					SheerResponse.Alert(Texts.AnErrorOcurred);
					return;
				}

				SheerResponse.ShowModalDialog(url, "720px", "470px", string.Empty, true);
				args.WaitForPostBack();
			}
			else if (args.HasResult)
			{
				string itemId;
				if (args.Result.IndexOf(',') >= 0)
				{
					string[] parts = args.Result.Split(',');
					itemId = parts[0];
				}
				else
				{
					itemId = args.Result;
				}

				XElement ruleset = this.RulesSet;
				string ruleId = args.Parameters["id"];
				XElement rule = GetRuleById(ruleset, ruleId);
				Assert.IsNotNull(rule, "rule");

				XElement action = GetActionById(rule, SetRenderingActionId) ?? AddAction(rule, SetRenderingActionId);

				action.SetAttributeValue("RenderingItem", ShortID.DecodeID(itemId));
				this.RulesSet = ruleset;
				var output = new HtmlTextWriter(new StringWriter());
				this.RenderSetRenderingAction(rule, output);
				SheerResponse.SetInnerHtml(ruleId + "_setrendering", output.InnerWriter.ToString().Replace("{ID}", ruleId));
			}
		}

		protected void SetRenderingClick([Sitecore.Annotations.NotNull] string id)
		{
			Assert.ArgumentNotNull(id, "id");
			if (!this.IsComponentDisplayed(id))
			{
				return;
			}

			var parameters = new NameValueCollection();
			parameters["id"] = id;
			Context.ClientPage.Start(this, "SetRendering", parameters);
		}

		protected void ShowConfirm([Sitecore.Annotations.NotNull] ClientPipelineArgs args)
		{
			Assert.ArgumentNotNull(args, "args");

			if (args.IsPostBack)
			{
				if (args.HasResult && args.Result != "no")
				{
					SheerResponse.Eval("scTogglePersonalizeComponentSection()");
					XElement rules = this.RulesSet;
					foreach (XElement rule in rules.Elements("rule"))
					{
						XElement setRenderingAction = GetActionById(rule, SetRenderingActionId);
						if (setRenderingAction != null)
						{
							setRenderingAction.Remove();
							var output = new HtmlTextWriter(new StringWriter());
							this.RenderSetRenderingAction(rule, output);
							ShortID ruleId = ShortID.Parse(rule.GetAttributeValue("uid"));
							Assert.IsNotNull(ruleId, "ruleId");

							SheerResponse.SetInnerHtml(
							  ruleId + "_setrendering", output.InnerWriter.ToString().Replace("{ID}", ruleId.ToString()));
						}
					}

					this.RulesSet = rules;
				}
				else
				{
					this.ComponentPersonalization.Checked = true;
				}
			}
			else
			{
				SheerResponse.Confirm(Texts.Personalizecomponentsettingswillberemove);
				args.WaitForPostBack();
			}
		}

		protected void SwitchRenderingClick([Sitecore.Annotations.NotNull] string id)
		{
			Assert.ArgumentNotNull(id, "id");

			XElement ruleSet = this.RulesSet;
			XElement rule = GetRuleById(ruleSet, id);
			if (rule != null)
			{
				if (!this.IsComponentDisplayed(rule))
				{
					RemoveAction(rule, HideRenderingActionId);
				}
				else
				{
					AddAction(rule, HideRenderingActionId);
				}

				this.RulesSet = ruleSet;
			}
		}

		[Sitecore.Annotations.NotNull]
		private static XElement AddAction([Sitecore.Annotations.NotNull] XElement rule, [Sitecore.Annotations.NotNull] string id)
		{
			Assert.ArgumentNotNull(rule, "rule");
			Assert.ArgumentNotNull(id, "id");

			var action = new XElement("action", new XAttribute("id", id), new XAttribute("uid", ID.NewID.ToShortID()));

			XElement actions = rule.Element("actions");
			if (actions == null)
			{
				rule.Add(new XElement("actions", action));
			}
			else
			{
				actions.Add(action);
			}

			return action;
		}

		[Sitecore.Annotations.CanBeNull]
		private static XElement GetActionById([Sitecore.Annotations.NotNull] XElement rule, [Sitecore.Annotations.NotNull] string id)
		{
			Assert.ArgumentNotNull(rule, "rule");
			Assert.ArgumentNotNull(id, "id");

			XElement actions = rule.Element("actions");
			if (actions == null)
			{
				return null;
			}

			return actions.Elements("action").FirstOrDefault(action => action.GetAttributeValue("id") == id);
		}

		[Sitecore.Annotations.CanBeNull]
		private static XElement GetRuleById([Sitecore.Annotations.NotNull] XElement ruleSet, [Sitecore.Annotations.NotNull] string id)
		{
			Assert.ArgumentNotNull(ruleSet, "ruleSet");
			Assert.ArgumentNotNull(id, "id");
			string uid = ID.Parse(id).ToString();

			return ruleSet.Elements("rule").FirstOrDefault(rule => rule.GetAttributeValue("uid") == uid);
		}

		[Sitecore.Annotations.NotNull]
		private static string GetRuleConditionsHtml([Sitecore.Annotations.NotNull] XElement rule)
		{
			Assert.ArgumentNotNull(rule, "rule");

			var output = new HtmlTextWriter(new StringWriter());
			var rules = new RulesRenderer("<ruleset>" + rule + "</ruleset>")
			{
				SkipActions = true
			};
			rules.Render(output);
			return output.InnerWriter.ToString();
		}

		private static bool IsDefaultCondition([Sitecore.Annotations.NotNull] XElement node)
		{
			Assert.ArgumentNotNull(node, "node");

			return node.GetAttributeValue("uid") == DefaultConditionId;
		}

		private static void RemoveAction([Sitecore.Annotations.NotNull] XElement rule, [Sitecore.Annotations.NotNull] string id)
		{
			Assert.ArgumentNotNull(rule, "rule");
			Assert.ArgumentNotNull(id, "id");

			XElement action = GetActionById(rule, id);
			if (action == null)
			{
				return;
			}

			action.Remove();
		}

		[Sitecore.Annotations.NotNull]
		private Menu GetActionsMenu([Sitecore.Annotations.NotNull] string id)
		{
			Assert.IsNotNullOrEmpty(id, "id");

			var actionsMenu = new Menu();
			actionsMenu.ID = id + "_menu";
			string icon = Images.GetThemedImageSource("office/16x16/delete.png");
			string click = "javascript:Sitecore.CollapsiblePanel.remove(this, event, \"{0}\")".FormatWith(id);
			actionsMenu.Add(Texts.DELETE, icon, click);
			icon = string.Empty;
			click = "javascript:Sitecore.CollapsiblePanel.renameAction(\"{0}\")".FormatWith(id);
			actionsMenu.Add(Texts.RENAME, icon, click);
			MenuDivider divider = actionsMenu.AddDivider();
			divider.ID = "moveDivider";

			icon = Images.GetThemedImageSource("ApplicationsV2/16x16/navigate_up.png");
			click = "javascript:Sitecore.CollapsiblePanel.moveUp(this, event, \"{0}\")".FormatWith(id);
			MenuItem menuItem = actionsMenu.Add(Texts.Moveup, icon, click);
			menuItem.ID = "moveUp";

			icon = Images.GetThemedImageSource("ApplicationsV2/16x16/navigate_down.png");
			click = "javascript:Sitecore.CollapsiblePanel.moveDown(this, event, \"{0}\")".FormatWith(id);
			menuItem = actionsMenu.Add(Texts.Movedown, icon, click);
			menuItem.ID = "moveDown";
			return actionsMenu;
		}

		[Sitecore.Annotations.NotNull]
		private string GetRuleSectionHtml([Sitecore.Annotations.NotNull] XElement rule)
		{
			Assert.ArgumentNotNull(rule, "rule");

			var writer = new HtmlTextWriter(new StringWriter());
			string id = ShortID.Parse(rule.GetAttributeValue("uid")).ToString();
			writer.Write("<table id='{ID}_body' cellspacing='0' cellpadding='0' class='rule-body'>");
			writer.Write("<tbody>");
			writer.Write("<tr>");

			writer.Write("<td class='left-column'>");
			this.RenderRuleConditions(rule, writer);
			writer.Write("</td>");

			writer.Write("<td class='right-column'>");
			this.RenderRuleActions(rule, writer);
			writer.Write("</td>");

			var afterActionPlaceholder = this.RenderRulePlaceholder(AfterActionPlaceholderName, rule);
			writer.Write(afterActionPlaceholder);

			writer.Write("</tr>");
			writer.Write("</tbody>");
			writer.Write("</table>");

			var panelHtml = writer.InnerWriter.ToString().Replace("{ID}", id);
			bool isDefault = IsDefaultCondition(rule);

			var actionsContext = new CollapsiblePanelRenderer.ActionsContext
			{
				IsVisible = !isDefault,
			};

			if (!isDefault)
			{
				actionsContext.OnActionClick = "javascript:return Sitecore.CollapsiblePanel.showActionsMenu(this,event)";
				actionsContext.Menu = this.GetActionsMenu(id);
			}

			string name = "Default";
			if (!isDefault || !string.IsNullOrEmpty(rule.GetAttributeValue("name")))
			{
				name = rule.GetAttributeValue("name");
			}

			var nameContext = new CollapsiblePanelRenderer.NameContext(name)
			{
				Editable = !isDefault,
				OnNameChanged = "javascript:return Sitecore.CollapsiblePanel.renameComplete(this,event)"
			};

			var panelRenderer = new CollapsiblePanelRenderer();
			panelRenderer.CssClass = "rule-container";
			return panelRenderer.Render(id, panelHtml, nameContext, actionsContext);
		}

		private string RenderRulePlaceholder(string placeholderName, XElement rule)
		{
			if (this.ContextItem == null)
			{
				return string.Empty;
			}

			var itemUri = this.ContextItem.Uri;
			var parsedDeviceId = ID.Parse(this.DeviceId);
			var ruleSetId = ID.Parse(this.RenderingDefition.UniqueId);

			return RenderRulePlaceholderPipeline.Run(placeholderName, itemUri, parsedDeviceId, ruleSetId, rule);

		}

		private bool IsComponentDisplayed([Sitecore.Annotations.NotNull] string id)
		{
			Assert.ArgumentNotNull(id, "id");

			XElement ruleSet = this.RulesSet;
			XElement rule = GetRuleById(ruleSet, id);
			if (rule != null)
			{
				if (!IsComponentDisplayed(rule))
				{
					return false;
				}
			}

			return true;
		}

		private bool IsComponentDisplayed([Sitecore.Annotations.NotNull] XElement rule)
		{
			Assert.ArgumentNotNull(rule, "rule");

			XElement hideRenderingAction = GetActionById(rule, HideRenderingActionId);
			if (hideRenderingAction != null)
			{
				return false;
			}

			return true;
		}

		private bool PersonalizeComponentActionExists()
		{
			XElement ruleSet = this.RulesSet;
			return ruleSet.Elements("rule").Any(rule => GetActionById(rule, SetRenderingActionId) != null);
		}

		private bool HasRules()
		{
			bool hasPersonalizedRule = false;

			foreach (XElement rule in RulesSet.Elements())
			{
				var ruleUniqueId = rule.GetAttributeValue(RulesDefinition.UidAttributeName);

				if (IsPersonalizedRule(ruleUniqueId))
				{
					hasPersonalizedRule = true;
					break;
				}
			}

			return hasPersonalizedRule;
		}

		private static bool IsPersonalizedRule(string ruleUniqueId)
		{
			return !string.IsNullOrEmpty(ruleUniqueId) && ID.Parse(ruleUniqueId) != ItemIDs.Null;
		}

		private void SetPersonalizationTrackingCheckbox(bool shouldSkipSupportedItemCheck = false, bool? checkboxState = null)
		{
			bool isSupportPersonalizationTracking = !shouldSkipSupportedItemCheck
				? IsItemSupportPersonalizationTracking
				: true;

			if (!isSupportPersonalizationTracking)
			{
				UnsupportedItemPersonalizationTrackingCheckboxState();
				return;
			}

			if (checkboxState.HasValue)
			{
				TogglePersonalizationCheckboxState(isChecked: checkboxState.Value);
				return;
			}

			bool hasPersonalizationRules = HasRules();

			if (!hasPersonalizationRules)
			{
				NoRulesPersonalizationCheckboxState();
				return;
			}

			bool hasPersonalizationTracking = HasPersonalizationTracking();

			TogglePersonalizationCheckboxState(isChecked: hasPersonalizationTracking);
		}

		private void UnsupportedItemPersonalizationTrackingCheckboxState()
		{
			PersonalizationTracking.Checked = false;
			PersonalizationTracking.Disabled = true;
			PersonalizationTracking.Visible = false;
		}

		private void NoRulesPersonalizationCheckboxState()
		{
			PersonalizationTracking.Checked = false;
			PersonalizationTracking.Disabled = true;
		}

		private void TogglePersonalizationCheckboxState(bool isChecked) => PersonalizationTracking.Checked = isChecked;

		private bool HasPersonalizationTracking()
		{
			bool hasPersonalizationTracking = false;

			if (!string.IsNullOrEmpty(RulesSet.GetAttributeValue("pet")))
			{
				hasPersonalizationTracking = bool.Parse(RulesSet.GetAttributeValue("pet"));
			}

			return hasPersonalizationTracking;
		}

		private bool PersonalizationTrackingCheckboxEnabled()
		{
			return PersonalizationTracking.Visible && !PersonalizationTracking.Disabled;
		}

		private void RenderHideRenderingAction([Sitecore.Annotations.NotNull] HtmlTextWriter writer, [Sitecore.Annotations.NotNull] string translatedText, bool isSelected, int index, [Sitecore.Annotations.CanBeNull] string style)
		{
			Assert.ArgumentNotNull(writer, "writer");

			var id = "hiderenderingaction_{ID}_" + index.ToString(CultureInfo.InvariantCulture);

			writer.Write("<input id='" + id + "' type='radio' name='hiderenderingaction_{ID}' onfocus='this.blur();' onchange=\"javascript:if (this.checked) { scSwitchRendering(this, event, '{ID}'); }\" ");
			if (isSelected)
			{
				writer.Write(" checked='checked' ");
			}

			if (!string.IsNullOrEmpty(style))
			{
				writer.Write(string.Format(CultureInfo.InvariantCulture, " style='{0}' ", style));
			}

			writer.Write("/>");

			writer.Write("<label for='" + id + "' class='section-header'>");
			writer.Write(translatedText);
			writer.Write("</label>");
		}

		private void RenderPicker([Sitecore.Annotations.NotNull] HtmlTextWriter writer, [Sitecore.Annotations.CanBeNull] Item item, [Sitecore.Annotations.NotNull] string clickCommand, [Sitecore.Annotations.NotNull] string resetCommand, bool prependEllipsis, bool notSet = false)
		{
			Assert.ArgumentNotNull(writer, "writer");
			Assert.ArgumentNotNull(clickCommand, "clickCommand");
			Assert.ArgumentNotNull(resetCommand, "resetCommand");

			string icon = Images.GetThemedImageSource(
			  item != null ? item.Appearance.Icon : string.Empty, ImageDimension.id16x16);

			string click = clickCommand + "(\\\"{ID}\\\")";
			string reset = resetCommand + "(\\\"{ID}\\\")";
			string name = Translate.Text(Texts.NOT_SET);
			string cssClass = "item-picker";
			if (item != null)
			{
				if (notSet)
				{
					name += prependEllipsis ? ".../" : string.Empty;
					name += " " + item.GetUIDisplayName();
				}
				else
				{
					name = prependEllipsis ? ".../" : string.Empty;
					name += item.GetUIDisplayName();
				}
			}
			if (item == null || notSet)
			{
				cssClass += " not-set";
			}

			writer.Write("<div style=\"background-image:url('{0}');background-position: left center;\" class='{1}'>", HttpUtility.HtmlEncode(icon), cssClass);
			writer.Write(
			  "<a href='#' class='pick-button' onclick=\"{0}\" title=\"{1}\">...</a>",
			  Context.ClientPage.GetClientEvent(click),
			  Translate.Text(Texts.SELECT));

			writer.Write(
			  "<a href='#' class='reset-button' onclick=\"{0}\" title=\"{1}\"></a>",
			  Context.ClientPage.GetClientEvent(reset),
			  Translate.Text(Texts.RESET));

			writer.Write("<span title=\"{0}\">{1}</span>", item == null ? string.Empty : item.GetUIDisplayName(), name);
			writer.Write("</div>");
		}

		private void RenderPicker([Sitecore.Annotations.NotNull] HtmlTextWriter writer, [Sitecore.Annotations.CanBeNull] string datasource, [Sitecore.Annotations.NotNull] string clickCommand, [Sitecore.Annotations.NotNull] string resetCommand, bool prependEllipsis, bool notSet = false)
		{
			Assert.ArgumentNotNull(writer, "writer");
			Assert.ArgumentNotNull(clickCommand, "clickCommand");
			Assert.ArgumentNotNull(resetCommand, "resetCommand");


			string click = clickCommand + "(\\\"{ID}\\\")";
			string reset = resetCommand + "(\\\"{ID}\\\")";
			string name = Translate.Text(Texts.NOT_SET);
			string cssClass = "item-picker";
			if (!datasource.IsNullOrEmpty())
			{
				if (notSet)
					name += " " + datasource;
				else
					name = datasource;
			}
			if (datasource.IsNullOrEmpty() || notSet)
			{
				cssClass += " not-set";
			}

			writer.Write(string.Format("<div class='{0}'>", cssClass));
			writer.Write(
			  "<a href='#' class='pick-button' onclick=\"{0}\" title=\"{1}\">...</a>",
			  Context.ClientPage.GetClientEvent(click),
			  Translate.Text(Texts.SELECT));

			writer.Write(
			  "<a href='#' class='reset-button' onclick=\"{0}\" title=\"{1}\"></a>",
			  Context.ClientPage.GetClientEvent(reset),
			  Translate.Text(Texts.RESET));
			var displayName = name;
			if (displayName != null)
			{
				if (displayName.Length > 15)
				{
					displayName = displayName.Substring(0, 14) + "...";
				}
			}

			writer.Write("<span title=\"{0}\">{1}</span>", name, displayName);
			writer.Write("</div>");
		}


		private void RenderRuleActions([Sitecore.Annotations.NotNull] XElement rule, [Sitecore.Annotations.NotNull] HtmlTextWriter writer)
		{
			Assert.ArgumentNotNull(rule, "rule");
			Assert.ArgumentNotNull(writer, "writer");

			var isComponentDisplayed = this.IsComponentDisplayed(rule);

			writer.Write("<div id='{ID}_hiderendering' class='hide-rendering'>");
			this.RenderHideRenderingAction(writer, Translate.Text(Texts.Show), isComponentDisplayed, 0, null);
			this.RenderHideRenderingAction(writer, Translate.Text(Texts.Hide), !isComponentDisplayed, 1, "margin-left:35px;");
			writer.Write("</div>");

			string cssClass = isComponentDisplayed ? string.Empty : " display-off";
			string style = this.ComponentPersonalization.Checked ? string.Empty : " style='display:none'";
			writer.Write("<div id='{ID}_setrendering' class='set-rendering" + cssClass + "'" + style + ">");
			this.RenderSetRenderingAction(rule, writer);
			writer.Write("</div>");

			writer.Write("<div id='{ID}_setdatasource' class='set-datasource" + cssClass + "'>");
			this.RenderSetDatasourceAction(rule, writer);
			writer.Write("</div>");
		}

		private void RenderRuleConditions([Sitecore.Annotations.NotNull] XElement rule, [Sitecore.Annotations.NotNull] HtmlTextWriter writer)
		{
			Assert.ArgumentNotNull(rule, "rule");
			Assert.ArgumentNotNull(writer, "writer");

			bool isDefault = IsDefaultCondition(rule);
			if (!isDefault)
			{
				var editButton = new Button
				{
					Header = Translate.Text(Texts.EDIT_RULE),
					ToolTip = Translate.Text(Texts.EDIT_THIS_RULE),
					Class = "scButton edit-button",
					Click = "EditConditionClick(\\\"{ID}\\\")"
				};
				writer.Write(HtmlUtil.RenderControl(editButton));
			}

			string cssClass = !isDefault ? "condition-container" : "condition-container default";
			writer.Write("<div id='{ID}_rule' class='" + cssClass + "'>");

			writer.Write(isDefault ? ConditionDescriptionDefault : GetRuleConditionsHtml(rule));
			writer.Write("</div>");
		}

		private void RenderRules()
		{
			var html = new StringBuilder();
			html.Append("<div id='non-default-container'>");
			foreach (XElement rule in this.RulesSet.Elements("rule"))
			{
				if (IsDefaultCondition(rule))
				{
					html.Append("</div>");
				}

				html.Append(this.GetRuleSectionHtml(rule));
			}

			this.RulesContainer.InnerHtml = html.ToString();
		}

		private void RenderSetDatasourceAction([Sitecore.Annotations.NotNull] XElement rule, [Sitecore.Annotations.NotNull] HtmlTextWriter writer)
		{
			Assert.ArgumentNotNull(rule, "rule");
			Assert.ArgumentNotNull(writer, "writer");

			string datasource = this.RenderingDefition.Datasource;
			XElement action = GetActionById(rule, SetDatasourceActionId);
			bool isDefaultValue = true;
			if (action != null)
			{
				datasource = action.GetAttributeValue("DataSource");
				isDefaultValue = false;
			}
			else
			{
				datasource = string.Empty;
			}

			Item item = null;
			var datasourceInferred = false;

			if (!string.IsNullOrEmpty(datasource))
			{
				item = Client.ContentDatabase.GetItem(datasource);
			}
			else
			{
				item = ContextItem;
				datasourceInferred = true;
			}

			writer.Write("<div " + (!isDefaultValue ? string.Empty : "class='default-values'") + ">");
			writer.Write("<span class='section-header' unselectable='on'>");
			writer.Write(Translate.Text(Texts.PersonalizeContent));
			writer.Write("</span>");
			const string ClickCommand = "SetDatasourceClick";
			const string ResetCommand = "ResetDatasource";

			if (item == null)
			{
				this.RenderPicker(writer, datasource, ClickCommand, ResetCommand, !datasourceInferred, datasourceInferred);
			}
			else
			{
				this.RenderPicker(writer, item, ClickCommand, ResetCommand, !datasourceInferred, datasourceInferred);
			}
			writer.Write("</div>");
		}

		private void RenderSetRenderingAction([Sitecore.Annotations.NotNull] XElement rule, [Sitecore.Annotations.NotNull] HtmlTextWriter writer)
		{
			Assert.ArgumentNotNull(rule, "rule");
			Assert.ArgumentNotNull(writer, "writer");

			string itemId = this.RenderingDefition.ItemID;
			XElement action = GetActionById(rule, SetRenderingActionId);
			bool isDefaultValues = true;
			if (action != null)
			{
				string value = action.GetAttributeValue("RenderingItem");
				if (!string.IsNullOrEmpty(value))
				{
					itemId = value;
					isDefaultValues = false;
				}
			}

			writer.Write("<div " + (!isDefaultValues ? string.Empty : "class='default-values'") + ">");
			if (string.IsNullOrEmpty(itemId))
			{
				writer.Write("</div>");
				return;
			}

			Item item = Client.ContentDatabase.GetItem(itemId);
			if (item == null)
			{
				writer.Write("</div>");
				return;
			}

			writer.Write("<span class='section-header' unselectable='on'>");
			writer.Write(Translate.Text(Texts.PersonalizeComponent));
			writer.Write("</span>");
			string src = Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id48x48);
			if (!string.IsNullOrEmpty(item.Appearance.Thumbnail) && item.Appearance.Thumbnail != Settings.DefaultThumbnail)
			{
				const int ThumbnailHeight = 128;
				const int ThumbnailWidth = 128;
				string thumbnailSrc = UIUtil.GetThumbnailSrc(item, ThumbnailHeight, ThumbnailWidth);

				if (!string.IsNullOrEmpty(thumbnailSrc))
				{
					src = thumbnailSrc;
				}
			}

			writer.Write("<div style=\"background-image:url('{0}')\" class='thumbnail-container'>", HttpUtility.HtmlEncode(src));
			writer.Write("</div>");

			writer.Write("<div class='picker-container'>");
			const string ClickCommand = "SetRenderingClick";
			const string ResetCommand = "ResetRendering";
			this.RenderPicker(writer, item, ClickCommand, ResetCommand, false);
			writer.Write("</div>");
			writer.Write("</div>");
		}

		#endregion
	}
}
