# Feature: Search #

The Habitat Home demo is built on SXA, and uses the built-in SXA search functionality.

### Using Solr

The following appSetting in the Web.config ensures that corresponding search configuration files for the Solr search provider are enabled

    <add key="search:define" value="Solr"/>

**SXA uses its own indexes by default**, so you you need to manually add the Solr cores for the SXA default indexes: 

`[site_prefix]_sxa_master_index`  
`[site_prefix]_sxa_web_index`

Update the following configuration file with the correct core names for **each** index:
`[webroot]\App_Config\Include\Z.Foundation.Overrides\Sitecore.XA.Foundation.Search.Solr.config`

example:

>` <param desc="core">habitathome_sxa_web_index</param>`


1. Restart Solr
1. Restart IIS
1. In the Sitecore Control Panel > Indexing
 - click "Populate Solr Managed Schema" and run it for the 2 new SXA indexes
 - click "Indexing manager" and rebuild the 2 new SXA indexes

**Enable Suggestions**

To enable suggestions, add the following configuration to the `solrconfig.xml` in each SXA solr index

    <searchComponent name="suggest" class="solr.SuggestComponent">
	    <lst name="suggester">
	      <str name="name">default</str>
	      <str name="field">name</str>
	      <str name="suggestAnalyzerFieldType">string</str>
	    </lst>
	    <lst name="suggester">
	      <str name="name">sxaSuggester</str>
	      <str name="field">sxacontent_txm</str>
	      <str name="suggestAnalyzerFieldType">text_en</str>
	    </lst>
    </searchComponent>
    
    <requestHandler name="/suggest" class="solr.SearchHandler" startup="lazy">
	    <lst name="defaults">
	      <str name="suggest">true</str>
	      <str name="suggest.count">10</str>
	      <str name="suggest.dictionary">sxaSuggester</str>
	    </lst>
	    <arr name="components">


Restart Solr


### Using Lucene
The Habitat Home demo has not been tested against Lucene indexes