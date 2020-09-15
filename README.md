# Welcome to the JP2 Portal Read Me!

This is a POC of a membership platform/ HR directory system, implemented in .NET MVC and MonogoDB. 

Link to restful service repo:  https://github.com/liendoanjp2/VEYMDataService

# Objective 

Interface for VEYM Users into Azure AD through Microsoft Graph and provide data tools for the organization.  

![](https://github.com/liendoanjp2/Jp2Portal/blob/master/Demo/Main%20Page.PNG)

# Technologies Used:   
 
(.NET) C# MVC Web Application 
(.NET) C# REST API, secured with OAuth 2.0 
MongoDB (For Sa Mac Back End) 
Azure AD Authentication 
Microsoft Graph API (in both v1.0 and Beta with elevated privileges: User.Read.All, Files.ReadWrite.All, Sites.ReadWrite.All, Mail.Send ) 

# Features: 
 
View your own Azure AD data: 
 
Directory Search: (example: Everyone with Last name Nguyen) 

Request User Data to be updated (ex. Cap 1 -> Cap 2) 
 
User Look Up (Currently only search by name works)  
 
Register for Training Camps: 
 
Admin Mode: (Create Training Camps, Drop SMS, Change Queue Priority of SMS)  

*Screenshots of the POC are found here:*

https://veym-my.sharepoint.com/:w:/g/personal/philips_nguyen_veym_net/EQJ31oeyLI1LktOpms_k6YIBLmSDgVWtUyM5Q_tZQaWpvA?e=vu7HpM

 
# Explanations of Use Cases that this solution is trying to solve: 
 
1. Sa Mac registration. 

We always receive more applicants than number of available spots for Cap 1. Since we can only accommodate for a set number of SMS. We need to fairly distribute camp slots. In this, as linked in the document. We have an old platform already written in razor pages + SQL running on a machine in a truong's basement. (Branded Mien Truong as well https://events.mientrungtntt.org/Event/Default.aspx). This allows SMS to create their own account for the platform and register for a camp. We are able to track their previous camps this way + manage the users registered for a given camp/their status by dropping them + updating a text field next to their name (ie. Awaiting payment). We are also able to see when a user signs up. But what is not implemented in a "Priority Queue System". Ideally, we want to be able to send SMS to the "back of the queue" if they do not meet expected pre-camp test results or do not turn in their forms in a timely manner. Then, we give that opportunity to another potential SMS. 

2. Directory look up. 

In the initial formation of LD, we really needed to hunt people's contact information. I know we have all this data in Azure AD. I think it would be awesome to be able to search HT based on Doan/LD/Rank. But as seen in the POC graph doesn't exactly support this... And we would have to read in the whole directly and run a search algorithm ourselves 

3. Helping the admin team with requests that require updating a user’s azure data. 

We get requests of HT wanting to update their azure AD data. (ex. someone's name is spelt incorrectly). 
 
In the screenshots, I show the use case where someone just Thang Cap and are requesting to change their rank. 
