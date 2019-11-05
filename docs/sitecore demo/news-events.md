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

### How To Add an Author and News Page in the Content Editor

1. Right click /sitecore/content/Habitat SXA Sites/{Your Site Name}/Data/Authors

1. Insert a new Author. Fill out the relevant information under Author.

1. Right click /sitecore/content/Habitat SXA Sites/{Your Site Name}/Home/news-events/News

1. Insert a new News Page. Fill out the relevant information under Content.

1. Your author will show up as a choice in the droptree for the Author field. Select the author here.

When you click on the author's name on a News Page, you will be redirected to the News landing page, which will show only the News Pages associated with that author.

### How to Add a News Page in the Experience Editor

Note: this is easiest to do from /sitecore/content/Habitat SXA Sites/{Your Site Name}/Home/news-events/News, but can technically be done from any page.

On the Home tab click on "Insert Page". In the content tree, navigate to the News landing page. Select the "News Page" type, enter a name, and click "Ok". A new News Page will appear in the Experience Editor, ready for editing.

### How to Change the Author on a News Page in the Experience Editor

On the News Page, click on the area that lists the author information. This is right below the breadcrumb navigation. An icon that looks like a person will appear on the left. Click this to change the author associated with this particular News Page.

### How to Add an Author in the Experience Editor

On the News Page, click on the area that lists the author information. This is right below the breadcrumb navigation. A blue icon that looks like a person with a plus sign will appear on the left. Click this to create a new author. After you create the author, the page will refresh. You can then change the associated author (see directions above) and edit the author's information.

## Events

Events are featured on the News and Events landing page, as well as the Events landing page. Events are contained in the Events data folder on the Global site. Event Pages are under your local site's Home node and are connected to the Event data items through "Link to Event's Page" on the Event data item. It is important to have this field correctly set.

The Upcoming Events and Event Calendar component list events according to the Events List they are pointed to. By default, this is the Demo Events list on the Global site. The associated list can be changed, as detailed further below.

### How To Add an Event and Event Page in the Content Editor

1. To add an Event Page, right click /sitecore/content/Habitat SXA Sites/{Your Site Name}/Home/news-events/Events and add a new Event Page.

1. To add an Event data item, right click an Event List under /sitecore/content/Habitat SXA Sites/Global/Data/Events.

1. Insert a Calendar Event. Fill out the relevant information under Event.

1. Set "Link to Event's Page" to the Event Page you created.

1. Navigate back to the Event Page. Click on "Details" under the "Presentation" tab.

1. Click on "Event Posting".

1. Set the Data Source to the event you created.

### How to Add an Event Page in the Experience Editor

Note: this is easiest to do from /sitecore/content/Habitat SXA Sites/{Your Site Name}/Home/news-events/Events, but can technically be done from any page.

On the Home tab click on "Insert Page". In the content tree, navigate to the Events landing page. Select the "Event Page" type, enter a name, and click "Ok". A new Event Page will appear in the Experience Editor, ready for editing.

### How to Change the Event on an Event Page in the Experience Editor

1. On the Event Page, click on the Event Posting rendering. Click on the checkmark icon in the middle.

1. Select "Change associated content".

1. Choose the desired event and click "Ok".

1. Be sure to change the event to point to the current event page. To do this, click on the left calendar icon on the Event Posting rendering.

### How to Create an Event in the Experience Editor

1. On the Event Page, click on the Event Posting rendering. Click on the checkmark icon in the middle.

1. Select "Change associated content".

1. Select an Events List. Click on "Create".

1. Now you can edit the new event in the Experience Editor. Be sure to point the event to the event page by clicking on the calendar icon.

### How to Change the Event List Associated with the Event Components

1. Navigate to /sitecore/content/Habitat SXA Sites/{Your Site Name}/Presentation/Partial Designs/Event Calendar

1. Open the Experience Editor. Select the event rendering.

1. Click on the middle checkmark icon. Select "Change associated content".

1. Select the Event List you wish to use. Save your changes.

1. Navigate to /sitecore/content/Habitat SXA Sites/{Your Site Name}/Presentation/Partial Designs/Upcoming Events

1. Repeat steps 2-4.
