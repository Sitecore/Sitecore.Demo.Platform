# News and Events Module

When creating a new Site, under Modules, be sure to select the following:

- Habitat Home XA Extensions
- Sitecore Demo News and Events

Paths to relevant Landing Pages:

/sitecore/content/Habitat SXA Sites/{Your Site Name}/Home/news-events

/sitecore/content/Habitat SXA Sites/{Your Site Name}/Home/news-events/Events

/sitecore/content/Habitat SXA Sites/{Your Site Name}/Home/news-events/News

## News

The search results for News Pages are featured on the News and Events landing page as the three News Pages with the most recent Publish Dates. The News landing page features all the News Pages in descending Publish Date order.

#### How To Add an Author and News Page in the Content Editor

1) Right click /sitecore/content/Habitat SXA Sites/{Your Site Name}/Data/Authors

2) Insert a new Author. Fill out the relevant information under Author.

3) Right click /sitecore/content/Habitat SXA Sites/{Your Site Name}/Home/news-events/News

4) Insert a new News Page. Fill out the relevant information under Content.

5) Your author will show up as a choice in the droptree for the Author field. Select the author here.

When you click on the author's name on a News Page, you will be redirected to the News landing page, which will show only the News Pages associated with that author.

#### How to Add a News Page in the Experience Editor

Note: this is easiest to do from /sitecore/content/Habitat SXA Sites/{Your Site Name}/Home/news-events/News, but can technically be done from any page.

On the Home tab click on "Insert Page". In the content tree, navigate to the News landing page. Select the "News Page" type, enter a name, and click "Ok". A new News Page will appear in the Experience Editor, ready for editing.

#### How to Change the Author on a News Page in the Experience Editor

On the News Page, click on the area that lists the author information. This is right below the breadcrumb navigation. An icon that looks like a person will appear on the left. Click this to change the author associated with this particular News Page.

## Events

Events are featured on the News and Events landing page, as well as the Events landing page. Events are contained in the Events data folder and can exist on your local site or in the Global site. Event Pages are under your local site's Home node and are connected to the Event data items through a field on both items - "Event" on the Event Page and "Link to Event's Page" on the Event data item. It is important to have both of these fields correctly set. 

The Upcoming Events component, featured on the News and Events landing page, will list Event Pages that are in your local site. However, the event calendar can only list events in a particular folder- that on your local site or the global site. By default, the event calendar's datasource is pointed to the global data folder. 

#### How To Add an Event and Event Page in the Content Editor

1) To add an Event data item, right click /sitecore/content/Habitat SXA Sites/{Your Site Name}/Data/Events. If an Event List does not exist, create one.

2) Right click on the Event List and create an Event. Fill out the relevant information under Event.

3) To add an Event Page, right click /sitecore/content/Habitat SXA Sites/{Your Site Name}/Home/news-events/Events and add a new Event Page.

4) Point the "Event" field to the Event data item you created.

5) Return to the Event data item and set "Link to Event's Page" to the Event Page you created.

#### How to Add an Events Page in the Experience Editor

Note: this is easiest to do from /sitecore/content/Habitat SXA Sites/{Your Site Name}/Home/news-events/Events, but can technically be done from any page.

On the Home tab click on "Insert Page". In the content tree, navigate to the Events landing page. Select the "Event Page" type, enter a name, and click "Ok". A new Event Page will appear in the Experience Editor, ready for editing.

#### How to Change the Event on an Event Page in the Experience Editor

On the Event Page, navigate to the "Experience Accelerator" tab. Click on "Other" in the Page Metadata section. Under Event, you can change the event associated with that particular Event Page.

