## {{buildDetails.buildNumber}}
{{buildDetails.startTime}} - {{buildDetails.id}}

{{#forEach workItems}}
{{#if isFirst}}#### Changes {{/if}}
{{#if (not (contains (lookup this.fields 'System.Tags') 'internal'))}}
*  [**{{this.id}}**](https://dev.azure.com/sitecoredst/Demo/_workitems/edit/**{{this.id}}**)  {{lookup this.fields 'System.Title'}}
{{/if}}
{{/forEach}}
