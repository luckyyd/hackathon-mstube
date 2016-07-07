package controllers

import javax.inject._
import play.api._
import play.api.mvc._

/**
 * This controller creates an `Action` to handle HTTP requests to the
 * application's home page.
 */
@Singleton
class HomeController @Inject() extends Controller {

  /**
   * Create an Action to render an HTML page with a welcome message.
   * The configuration in the `routes` file means that this method
   * will be called when the application receives a `GET` request with
   * a path of `/`.
   */
  def index = Action {
    Ok(views.html.index("Your new application is ready."))
  }
  
  def getCandidates = Action {
      Ok(""""Shows": [
{
    "title": "Microsoft Azure Cloud Cover Show",
    "id": 1,
    "url": "https://channel9.msdn.com/Shows/Cloud+Cover",
    "image_src": "https://f.ch9.ms/thumbnail/a4e902b2-c5de-49a0-82ef-d7f5a7b960e8.png",
    "crawled_time": "2016-07-06 14:19"
}, {
   "title": "Azure Friday",
    "id": 2,
    "url": "https://channel9.msdn.com/Shows/Azure-Friday",
    "image_src": "https://f.ch9.ms/thumbnail/a5c7cd91-1a04-45ff-9822-a1197ee46841.png",
    "crawled_time": "2016-07-06 14:19"
}, {
    "title": "Subscribe!",
    "id": 3,
    "url": "https://channel9.msdn.com/Blogs/Subscribe",
    "image_src": "https://f.ch9.ms/thumbnail/815543c1-f096-4771-9b49-d0a3f7a74095.png",
    "crawled_time": "2016-07-06 14:19"
}, {
    "title": "Azure App Service",
    "id": 4,
    "url": "https://channel9.msdn.com/Series/Windows-Azure-Web-Sites-Tutorials",
    "image_src": "https://f.ch9.ms/thumbnail/ffe99e05-c6be-48cd-9bdb-29fc75ba79f1.png",
    "crawled_time": "2016-07-06 14:19"
}, {
    "title": "Windows Azure Active Directory",
    "id": 5,
    "url": "https://channel9.msdn.com/Series/Windows-Azure-Active-Directory",
    "image_src": "https://f.ch9.ms/thumbnail/0e5b8943-3c94-4ad0-8e80-72e7cd5c6a22.png",
    "crawled_time": "2016-07-06 14:19"
}]
""")
  }

}
