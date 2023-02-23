WebApp1 is an asp.net core mvc project, that was made with udemy course [Build real world application using ASP.NET Core MVC, Entity Framework Core and ASP.NET Core Identity](https://ua.udemy.com/course-dashboard-redirect/?course_id=1844356).
All the sensitive data were placed in [appsettings.json](BookStore/appsettings.json). In addition to that program code was refactorized and some bugs in the project were fixed.

WebApp1 is a book shop, which has basic functionality(buy a product, track your order and cancel it) from a usual customer side and a feature to pay fro the order in a while, if you're authorized as a company user.

In case you'd like to use the website, here's the [link](https://webbookstore1.azurewebsites.net/) for it. Admin features will be available if you use email and password of admin in [appsettings.json](BookStore/appsettings.json).

To download and use the project on your local machine you have to fill up all the fields marked as ? in [appsettings.json](BookStore/appsettings.json), that includes as well a registration on such services as [Meta fro developers](https://developers.facebook.com/) (to enable authorizing via facebook), [Stripe](https://stripe.com/en-cz) (to enable payment). Email and password fields in the config file are required to send confirmation emails to the users of the website.   
