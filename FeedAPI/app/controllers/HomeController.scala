package controllers

import java.io._
import java.util._

import scala.collection.JavaConversions._
import scala.io._

import javax.inject._
import play.api._
import play.api.mvc._
import play.api.Play.current

import play.api.libs.json._ // JSON library
import play.api.libs.json.Reads._ // Custom validation helpers
import play.api.libs.functional.syntax._ // Combinator syntax

import org.apache.mahout.cf.taste.impl.model._
import org.apache.mahout.cf.taste.impl.model.file._
import org.apache.mahout.cf.taste.impl.neighborhood._
import org.apache.mahout.cf.taste.impl.recommender._
import org.apache.mahout.cf.taste.impl.similarity._
import org.apache.mahout.cf.taste.model._
import org.apache.mahout.cf.taste.neighborhood._
import org.apache.mahout.cf.taste.recommender._
import org.apache.mahout.cf.taste.similarity._

import com.redis._

case class Item(itemID: Long, title: String, description: String, image_src: String, url: String)

object Item {
    
    implicit val itemReads: Reads[Item] = (
      (JsPath \\ "id").read[Long] and 
      (JsPath \\ "title").read[String] and 
      (JsPath \\ "description").read[String] and 
      (JsPath \\ "image_src").read[String] and 
      (JsPath \\ "url").read[String]
    )(Item.apply _)
            
    implicit val itemWrites = new Writes[Item] {
        def writes(i: Item): JsValue = 
            Json.obj(
                "id" -> i.itemID,
                "title" -> i.title,
                "description" -> i.description,
                "image_src" -> i.image_src,
                "url" -> i.url
            ) 
    } 
}


object HomeController {
    
    private val howMany = 5
    private val n = 5 // Nearest N User Neighborhood
    private val pref_file = "prefs.csv"
    private val item_file = "app/assets/items.json"

    private var items: Seq[Item] = null

    private def getItems() : Seq[Item] = {
        
        if (items == null)
        {
            val source: String = Source.fromFile(item_file)("UTF-8").getLines.mkString
            val json: JsValue = Json.parse(source)
            
            items = json.as[Seq[Item]]
        }

        items

    }
    
    private def getPrefFile() : File = {
        
        var file: File = new File(pref_file)
        
        if(!file.exists())
        {
            val bw = new BufferedWriter(new FileWriter(file))
            bw.write("1,1,1.0")
            bw.newLine()
            bw.close
            
            file = new File(pref_file)
        }
        
        file
    }

    private def recommend(userID: Long) : List[Long] = {
        
        println("UserID:" + userID)
        
        var file: File = getPrefFile()
        println("Pref file's absolute path = " + file.getAbsolutePath)
        println("Pref file exists: " + file.exists)
        
        var model: GenericBooleanPrefDataModel = new GenericBooleanPrefDataModel(
				GenericBooleanPrefDataModel.toDataMap(new FileDataModel(new File(pref_file))))

		var similarity: UserSimilarity = new LogLikelihoodSimilarity(model)
		var neighborhood: UserNeighborhood = new NearestNUserNeighborhood(n, similarity, model);
	
		var recommender: Recommender = new GenericUserBasedRecommender(model, neighborhood, similarity)
		var recommendations = recommender.recommend(userID, howMany)

        for (r <- recommendations) yield r.getItemID

    }
    
    private def getCandidates(userID: Long) : Seq[Item] = {
        
        val items: Seq[Item]  = getItems
        val itemIDs: List[Long] = recommend(userID)
        
        val candidates: Seq[Item] = items.filter(i => itemIDs.contains(i.itemID))
        
        if (candidates.size > 0)
        {
            candidates
        }
        else
        {
            items.take(howMany)
        }

    }
}



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
      
      var userID: Long = 1L
      
      val candidates: Seq[Item] = HomeController.getCandidates(userID)
      val jsonArray = Json.toJson(candidates)
      
      println("Hello World")
        
      Ok(Json.stringify(jsonArray))
      
  }
  
  def connectRedis = Action {
      
      
      try { 
        val redis = new RedisClient("hackathon-mstube.redis.cache.windows.net", 6379, secret=Some("sXyQJyy39j1GmfqslvY9I1czfi+PemKepnqc5PR13XU="))
        
        //Read
        val result = redis.get("2 2")
        println(result)
        
        //Write
        redis.set("3 3", "3")
       } catch {
         case e: Exception => println("Got some other kind of exception")
       }
      
      
      Ok(views.html.index("Your redis."))
  }
  
  def addPreference = Action(parse.json) {
      
      request => 
      {
          var json: JsValue = request.body
          
          var user_id = (json \ "user id").as[Long]
          var item_id = (json \ "item id").as[Float]
          var pref = (json \ "pref").as[Float]
          
          var file = new File(HomeController.pref_file)
          var bw = new BufferedWriter(new FileWriter(file, true))
          bw.write("%d,%d,%f".format(user_id,item_id,pref))
          bw.newLine()
          bw.close()
          
          Ok(user_id.toString)
           
      }
  }

}
