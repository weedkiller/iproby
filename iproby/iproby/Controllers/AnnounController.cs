﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iproby.Data_Model;
using iproby.Models;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;

namespace iproby.Controllers
{
    public class AnnounController : Controller
    {
        //
        // GET: /Announ/
        private iproby94_cust_dbEntities db = new iproby94_cust_dbEntities();
        private static bool isSaved=false;
        private static bool notLogin = false;
        
        public ActionResult Index(int announ_id)
        {
            if (isSaved)
            {
                ViewData["isSaved"] = "isSaved";
                isSaved = false;
            }
            if (notLogin)
            {
                ViewData["notLogin"] = "notLogin";
                notLogin = false;
            }
            var announ_arr = (from a in db.announs
                              where a.id == announ_id
                              select a);
            iproby.Models.announ_full announ = new iproby.Models.announ_full();
            foreach (var item_inside in announ_arr)
            {
                announ.description = item_inside.description;
                announ.header = item_inside.header;
                announ.announ_id = item_inside.id;
                announ.about = item_inside.about;
                announ.about_truncated = TruncateAtWord(item_inside.about,135);
                announ.price = item_inside.price;
                announ.subjects = item_inside.subjects;
                announ.type_id = item_inside.type_id.Value;
            }
            var customer_id_arr = (from a in db.customer_announ
                                   where a.announ_id == announ_id
                                   select a);
            int customer_id = 0;
            DateTime date_from = DateTime.Now;
            foreach (var item_inside in customer_id_arr)
            {
                customer_id = item_inside.customer_id.Value;
                date_from = item_inside.date_from;
            }
            announ.customer_id = customer_id;
            var contact_id_arr = (from a in db.customers
                                  where a.customer_id == customer_id
                                  select a.contact_id);
            int contact_id = 0;
            foreach (int item_inside in contact_id_arr)
            {
                contact_id = item_inside;
            }
            var contact_arr = (from a in db.contacts
                               where a.contact_id == contact_id
                               select a);
            foreach (var item_inside in contact_arr)
            {
                announ.first_name = item_inside.first_name;
                announ.mobile = item_inside.mobile;
                announ.address = item_inside.address;
                announ.skype = item_inside.skype;
                announ.email = item_inside.email;
                announ.avatar = item_inside.avatar;
                announ.site = item_inside.site;
                announ.facebook = item_inside.facebook;
                announ.icq = item_inside.icq;
                announ.vkontakte = item_inside.vkontakte;
                announ.date_from = date_from;
               
            }
            var target_arr = (from a in db.announ_target
                              where a.announ_id == announ_id
                               select a.target_type);
            string target = string.Empty;
            foreach (string item in target_arr)
            {
                target = item;
                announ.target = target;
            }
            
            return View(announ);
        }

        public ActionResult Reviews(int announ_id)
        {

            var reviews_arr = (from a in db.reviews
                               where a.announ_id == announ_id
                               select a);
            List<iproby.Models.review> review_list = new List<iproby.Models.review>();
            foreach (var item in reviews_arr)
            {
                iproby.Models.review review = new iproby.Models.review();
                review.header = item.header;
                review.description = item.description;
                review.date_from = item.date_from;
                review.reviewer_id = item.reviewer_id;
                var contact_id_arr = (from a in db.customers
                                       where a.customer_id == item.reviewer_id
                                       select a.contact_id);
                int contact_id = 0;
                foreach (int item_customer in contact_id_arr)
                {
                    contact_id = item_customer;
                }
                var contact_arr = (from a in db.contacts
                               where a.contact_id == contact_id
                               select a);
                foreach (var item_inside in contact_arr)
                {
                    review.first_name = item_inside.first_name;
                    review.address = item_inside.address;
                    review.avatar = item_inside.avatar;
                }
                review_list.Add(review);
            }
            return View("~/Views/Announ/ReviewsList.cshtml",review_list);
        }

        [HttpPost]
        public string GetChildTypes(string parent_type)
        {
            List<string> type_list = new List<string>();
            if (parent_type == null)
            {
                type_list.Add("Не найден");
                ViewBag.type_arr = type_list.ToList();
                JavaScriptSerializer JsSerializer = new JavaScriptSerializer();
                return JsSerializer.Serialize(type_list);
            }
            else { 
                    var type_arr = (from a in db.announ_type
                                    where a.parent_type.ToLower() == parent_type.ToLower()
                                           select a.type);
                    if (type_arr.Count() == 0) {
                        type_list.Add("Не найден");
                    }
                    JavaScriptSerializer JsSerializer = new JavaScriptSerializer();
                    return JsSerializer.Serialize(type_arr);
                  }
        }
        public ActionResult AddClients()
        {
                var parent_type_arr = (from a in db.announ_type
                                        select a.parent_type).Distinct();
                ViewBag.parent_type_arr = parent_type_arr.ToList();
                iproby.Models.announ_clients model = new iproby.Models.announ_clients();
                
                return View(model);
        }
        public ActionResult AddWorkers()
        {
            var parent_type_arr = (from a in db.announ_type
                                   select a.parent_type).Distinct();
            ViewBag.parent_type_arr = parent_type_arr.ToList();
            iproby.Models.announ_clients model = new iproby.Models.announ_clients();

            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult AddClients(iproby.Models.announ_clients model)
        {
            if (Session["login"] != null)
            {
                iproby.Data_Model.announ announ = new iproby.Data_Model.announ();
                announ.about = model.about;
                var type_id_arr = (from a in db.announ_type
                                   where a.type == model.type
                                   select a.id);
                int type_id = 0;
                foreach (int item in type_id_arr)
                {
                    type_id = item;
                }
                announ.type_id = type_id;
                announ.description = model.description.Replace("&lt;", "").Replace("&gt;", "");
                announ.header = model.header;
                announ.subjects = model.subjects;
                announ.price = model.price;
                db.announs.Add(announ);
                db.SaveChanges();

                iproby.Data_Model.customer_announ customer_announ = new iproby.Data_Model.customer_announ();
                customer_announ.announ_id = announ.id;
                string login = Session["login"].ToString();
                var customer_id_arr = (from a in db.customers
                                       where a.login == login
                                       select a.customer_id);
                int customer_id = 0;
                foreach (int item in customer_id_arr)
                {
                    customer_id = item;
                }
                customer_announ.customer_id = customer_id;
                DateTime Now = DateTime.Now;
                customer_announ.date_from = Now;
                string dateToString = "09/09/9999";
                DateTime dateTo = DateTime.Parse(dateToString, System.Globalization.CultureInfo.InvariantCulture);
                customer_announ.date_to = dateTo;
                customer_announ.active = 1;
                db.customer_announ.Add(customer_announ);
                db.SaveChanges();
                iproby.Data_Model.announ_target announ_target = new iproby.Data_Model.announ_target();
                announ_target.target_type="workers";
                announ_target.announ_id = announ.id;
                db.announ_target.Add(announ_target);
                db.SaveChanges();
                iproby.Data_Model.option options = new iproby.Data_Model.option();
                options.customer_id = customer_id;
                options.send_email_from_clients_flag = 1;
                db.options.Add(options);
                db.SaveChanges();
                service_model service_model = new service_model();
                service_model.announ_id = announ.id;
                return View("~/Views/Status/AddAnnounSuccess.cshtml", service_model);
            }
            else {
                return View("~/Views/Status/NoAuthorization.cshtml");
            }
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult AddWorkers(iproby.Models.announ_clients model)
        {
            if (Session["login"] != null)
            {
                iproby.Data_Model.announ announ = new iproby.Data_Model.announ();
                announ.about = model.about;
                var type_id_arr = (from a in db.announ_type
                                   where a.type == model.type
                                   select a.id);
                int type_id = 0;
                foreach (int item in type_id_arr)
                {
                    type_id = item;
                }
                announ.type_id = type_id;
                announ.description = model.description.Replace("&lt;", "").Replace("&gt;", "");
                announ.header = model.header;
                announ.subjects = model.subjects;
                announ.price = model.price;
                db.announs.Add(announ);
                db.SaveChanges();

                iproby.Data_Model.customer_announ customer_announ = new iproby.Data_Model.customer_announ();
                customer_announ.announ_id = announ.id;
                string login = Session["login"].ToString();
                var customer_id_arr = (from a in db.customers
                                       where a.login == login
                                       select a.customer_id);
                int customer_id = 0;
                foreach (int item in customer_id_arr)
                {
                    customer_id = item;
                }
                customer_announ.customer_id = customer_id;
                DateTime Now = DateTime.Now;
                customer_announ.date_from = Now;
                string dateToString = "09/09/9999";
                DateTime dateTo = DateTime.Parse(dateToString, System.Globalization.CultureInfo.InvariantCulture);
                customer_announ.date_to = dateTo;
                customer_announ.active = 1;
                db.customer_announ.Add(customer_announ);
                db.SaveChanges();
                iproby.Data_Model.announ_target announ_target = new iproby.Data_Model.announ_target();
                announ_target.target_type = "clients";
                announ_target.announ_id = announ.id;
                db.announ_target.Add(announ_target);
                db.SaveChanges();
                service_model service_model= new service_model();
                service_model.announ_id=announ.id;
                return View("~/Views/Status/AddAnnounSuccess.cshtml", service_model);
            }
            else
            {
                return View("~/Views/Status/NoAuthorization.cshtml");
            }
        }

        public int GetReviews(int announ_id)
        {
            int reviews_count = (from a in db.reviews
                             where a.announ_id == announ_id
                             select a).Count();
            return reviews_count;
        }

        public ActionResult SearchResultInTypes(string search_text, int type_id=0)
        {
            ViewBag.type_id = type_id;
            if (type_id != 0)
            {
                if (search_text.Length > 2 && search_text.Length < 200)
                {
                    iproby.Data_Model.search_texts search_text_db = new iproby.Data_Model.search_texts();
                    search_text_db.search_input = search_text;
                    DateTime date_from1 = DateTime.Now;
                    search_text_db.date_from = date_from1;
                    db.search_texts.Add(search_text_db);
                    db.SaveChanges();
                    var announ_id_arr = (from a in db.announs
                                         where ((a.description.Contains(search_text)
                                         || a.header.Contains(search_text)
                                         || a.about.Contains(search_text)
                                         || a.subjects.Contains(search_text))
                                         && a.type_id == type_id)
                                         select a);
                    int announ_id = 0;
                    List<iproby.Models.announ_preview> all_announs = new List<iproby.Models.announ_preview>();
                    foreach (var item in announ_id_arr)
                    {
                        announ_id = item.id;
                        var announ_arr = (from a in db.announs
                                          where a.id == announ_id
                                          select a);
                        iproby.Models.announ_preview announ = new iproby.Models.announ_preview();
                        announ.from_search_flag = 1;
                        announ.type_id = type_id;
                        foreach (var item_inside in announ_arr)
                        {
                            announ.description = TruncateAtWord(SkipHtml(item_inside.description.Trim()), 360);
                            announ.header = item_inside.header;
                            announ.announ_id = item_inside.id;
                        }
                        var customer_id_arr = (from a in db.customer_announ
                                               where a.announ_id == announ_id
                                               select a);
                        int customer_id = 0;
                        DateTime date_from = DateTime.Now;
                        foreach (var item_inside in customer_id_arr)
                        {
                            customer_id = item_inside.customer_id.Value;
                            date_from = item_inside.date_from;
                        }
                        announ.customer_id = customer_id;
                        var contact_id_arr = (from a in db.customers
                                              where a.customer_id == customer_id
                                              select a.contact_id);
                        int contact_id = 0;
                        foreach (int item_inside in contact_id_arr)
                        {
                            contact_id = item_inside;
                        }
                        var contact_arr = (from a in db.contacts
                                           where a.contact_id == contact_id
                                           select a);
                        foreach (var item_inside in contact_arr)
                        {
                            announ.first_name = item_inside.first_name;
                            announ.mobile = item_inside.mobile;
                            announ.address = item_inside.address;
                            announ.date_from = date_from;
                            announ.search_word = search_text;
                            announ.email = item_inside.email;
                            announ.vkontakte = (item_inside.vkontakte != null) ? item_inside.vkontakte : "не указан";
                            announ.skype = (item_inside.skype != null) ? item_inside.skype : "не указан";
                            announ.facebook = (item_inside.facebook != null) ? item_inside.facebook : "не указан";
                            announ.address = item_inside.address;
                            announ.avatar = item_inside.avatar_cropped;
                            announ.date_from = date_from;
                        }
                        all_announs.Add(announ);
                    }

                    return View("~/Views/Catalog/Announs.cshtml", all_announs);
                }
                else
                {
                    List<iproby.Models.announ_preview> all_announs = new List<iproby.Models.announ_preview>();
                    return View("~/Views/Catalog/Announs.cshtml", all_announs);
                }
            }
            else {
                return SearchResultAll(search_text);
            }
        }

        public ActionResult SearchResultAll(string search_text)
        {
            if (search_text.Length > 2 && search_text.Length < 200)
            {
                iproby.Data_Model.search_texts search_text_db = new iproby.Data_Model.search_texts();
                search_text_db.search_input = search_text;
                DateTime date_from1 = DateTime.Now;
                search_text_db.date_from = date_from1;
                db.search_texts.Add(search_text_db);
                db.SaveChanges();
                var announ_id_arr = (from a in db.announs
                                     where (a.description.Contains(search_text)
                                     || a.header.Contains(search_text)
                                     || a.about.Contains(search_text)
                                     || a.subjects.Contains(search_text))
                                     select a);
                int announ_id = 0;
                List<iproby.Models.announ_preview> all_announs = new List<iproby.Models.announ_preview>();
                foreach (var item in announ_id_arr)
                {
                    announ_id = item.id;
                    var announ_arr = (from a in db.announs
                                      where a.id == announ_id
                                      select a);
                    iproby.Models.announ_preview announ = new iproby.Models.announ_preview();
                    announ.from_search_flag = 1;
                    foreach (var item_inside in announ_arr)
                    {
                        announ.description = TruncateAtWord(SkipHtml(item_inside.description.Trim()), 360);
                        announ.header = item_inside.header;
                        announ.announ_id = item_inside.id;
                        announ.type_id = item_inside.type_id.Value;
                    }
                    var customer_id_arr = (from a in db.customer_announ
                                           where a.announ_id == announ_id
                                           select a);
                    int customer_id = 0;
                    DateTime date_from = DateTime.Now;
                    foreach (var item_inside in customer_id_arr)
                    {
                        customer_id = item_inside.customer_id.Value;
                        date_from = item_inside.date_from;
                    }
                    announ.customer_id = customer_id;
                    var contact_id_arr = (from a in db.customers
                                          where a.customer_id == customer_id
                                          select a.contact_id);
                    int contact_id = 0;
                    foreach (int item_inside in contact_id_arr)
                    {
                        contact_id = item_inside;
                    }
                    var contact_arr = (from a in db.contacts
                                       where a.contact_id == contact_id
                                       select a);
                    foreach (var item_inside in contact_arr)
                    {
                        announ.first_name = item_inside.first_name;
                        announ.mobile = item_inside.mobile;
                        announ.address = item_inside.address;
                        announ.date_from = date_from;
                        announ.search_word = search_text;
                        announ.email = item_inside.email;
                        announ.vkontakte = (item_inside.vkontakte != null) ? item_inside.vkontakte : "не указан";
                        announ.skype = (item_inside.skype != null) ? item_inside.skype : "не указан";
                        announ.facebook = (item_inside.facebook != null) ? item_inside.facebook : "не указан";
                        announ.address = item_inside.address;
                        announ.avatar = item_inside.avatar_cropped;
                        announ.date_from = date_from;

                    }
                    all_announs.Add(announ);
                }

                return View("~/Views/Catalog/Announs.cshtml", all_announs);
            }
            else {
                List<iproby.Models.announ_preview> all_announs = new List<iproby.Models.announ_preview>();
                return View("~/Views/Catalog/Announs.cshtml", all_announs);
            }
        }

        public ActionResult SearchResultAllTarget(string search_text, string target="workers", int type_id=0)
        {
            if (search_text.Length > 2 && search_text.Length < 200)
            {
                iproby.Data_Model.search_texts search_text_db = new iproby.Data_Model.search_texts();
                search_text_db.search_input = search_text;
                DateTime date_from1 = DateTime.Now;
                search_text_db.date_from = date_from1;
                db.search_texts.Add(search_text_db);
                db.SaveChanges();
                var announ_id_arr = Enumerable.Empty<iproby.Data_Model.announ>().AsQueryable();
                if (type_id == 0)
                {
                     announ_id_arr = (from a in db.announs
                                         where (a.description.Contains(search_text)
                                         || a.header.Contains(search_text)
                                         || a.about.Contains(search_text)
                                         || a.subjects.Contains(search_text))
                                         join db_target in db.announ_target on a.id equals db_target.announ_id
                                         where db_target.target_type.Contains(target)
                                         select a);
                }
                else {
                     announ_id_arr = (from a in db.announs
                                         where ((a.description.Contains(search_text)
                                         || a.header.Contains(search_text)
                                         || a.about.Contains(search_text)
                                         || a.subjects.Contains(search_text))
                                         && a.type_id == type_id)
                                         join db_target in db.announ_target on a.id equals db_target.announ_id
                                         where db_target.target_type.Contains(target)
                                         select a);
                }
                int announ_id = 0;
                List<iproby.Models.announ_preview> all_announs = new List<iproby.Models.announ_preview>();
                foreach (var item in announ_id_arr)
                {
                    announ_id = item.id;
                    var announ_arr = (from a in db.announs
                                      where a.id == announ_id
                                      select a);
                    iproby.Models.announ_preview announ = new iproby.Models.announ_preview();
                    announ.from_search_flag = 1;
                    foreach (var item_inside in announ_arr)
                    {
                        announ.description = TruncateAtWord(SkipHtml(item_inside.description.Trim()), 360);
                        announ.header = item_inside.header;
                        announ.announ_id = item_inside.id;
                        announ.type_id = item_inside.type_id.Value;
                    }
                    var customer_id_arr = (from a in db.customer_announ
                                           where a.announ_id == announ_id
                                           select a);
                    int customer_id = 0;
                    DateTime date_from = DateTime.Now;
                    foreach (var item_inside in customer_id_arr)
                    {
                        customer_id = item_inside.customer_id.Value;
                        date_from = item_inside.date_from;
                    }
                    announ.customer_id = customer_id;
                    var contact_id_arr = (from a in db.customers
                                          where a.customer_id == customer_id
                                          select a.contact_id);
                    int contact_id = 0;
                    foreach (int item_inside in contact_id_arr)
                    {
                        contact_id = item_inside;
                    }
                    var contact_arr = (from a in db.contacts
                                       where a.contact_id == contact_id
                                       select a);
                    foreach (var item_inside in contact_arr)
                    {
                        announ.first_name = item_inside.first_name;
                        announ.mobile = item_inside.mobile;
                        announ.address = item_inside.address;
                        announ.date_from = date_from;
                        announ.search_word = search_text;
                        announ.email = item_inside.email;
                        announ.vkontakte = (item_inside.vkontakte != null) ? item_inside.vkontakte : "не указан";
                        announ.skype = (item_inside.skype != null) ? item_inside.skype : "не указан";
                        announ.facebook = (item_inside.facebook != null) ? item_inside.facebook : "не указан";
                        announ.address = item_inside.address;
                        announ.avatar = item_inside.avatar_cropped;
                        announ.date_from = date_from;

                    }
                    announ.from_search_flag = 1;
                    all_announs.Add(announ);
                }
                ViewData["search_text"] = search_text;

                return View("~/Views/Catalog/Announs.cshtml", all_announs);
            }
            else
            {
                List<iproby.Models.announ_preview> all_announs = new List<iproby.Models.announ_preview>();
                return View("~/Views/Catalog/Announs.cshtml", all_announs);
            }
        }

        [HttpPost]
        public ActionResult AddRequest(iproby.Models.request model)
        {
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public ActionResult AddReview(iproby.Models.review model)
        {
            if (Session["login"] != null)
            {
                string login = Session["login"].ToString();
                var customer_id_arr = (from a in db.customers
                                       where a.login == login
                                       select a.customer_id);
                int customer_id = 0;
                foreach (int item in customer_id_arr)
                {
                    customer_id = item;
                }
                iproby.Data_Model.review review = new iproby.Data_Model.review();
                review.reviewer_id = customer_id;
                review.announ_id = model.announ_id;
                review.customer_id = model.customer_id;
                var to_contact_id_arr = (from a in db.customers
                                       where a.customer_id == model.customer_id
                                          select a.contact_id);
                int to_contact_id = 0;
                foreach (int item in to_contact_id_arr)
                {
                    to_contact_id = item;
                }
                var contact_arr = (from a in db.contacts
                                       where a.contact_id == to_contact_id
                                   select a);
                string email = string.Empty;
                foreach (var item in contact_arr)
                {
                    email = item.email;
                }
                review.header = model.header;
                review.description = model.description;
                DateTime Now = DateTime.Now;
                review.date_from = Now;
                db.reviews.Add(review);
                db.SaveChanges();
                InformationController notification = new InformationController();
                notification.SendMail(email, 
                @"К вашему объявлению добавили комментарий на сайте IPRO. 
                Пожалуйста, проверьте в личном кабинете и по возможости напишите ответ (ответить автору сообщения возможно в Личном кабинете).

                Письмо сгенерировано автоматически. По всем вопросам пишите на info@iproby.ru");
                isSaved = true;
                return RedirectToAction("Index", new { announ_id = model.announ_id });
            }
            else {
                notLogin = true;
                return RedirectToAction("Index", new { announ_id = model.announ_id });
            }
        }

        public int GetLikes(int announ_id)
        {
            var likes_arr = (from a in db.likes
                             where a.announ_id == announ_id
                             select a.like_num);
            int like_num = 0;
            foreach (int item in likes_arr)
            {
                like_num = item;
            }
            return like_num;
        }

        public int GetDislikes(int announ_id)
        {
            var likes_arr = (from a in db.likes
                             where a.announ_id == announ_id
                             select a.disline_num);
            int dislike_num = 0;
            foreach (int item in likes_arr)
            {
                dislike_num = item;
            }
            return dislike_num;
        }

        [HttpPost]
        public int AddLike(int announ_id)
        {
            //if (Session["login"] != null)
            //{
                //string login = Session["login"].ToString();
                //var customer_id_arr = (from a in db.customers
                //                       where a.login == login
                //                       select a.customer_id);
                //int customer_id = 0;
                //foreach (int item in customer_id_arr)
                //{
                //    customer_id = item;
                //}
                var likes_arr = (from a in db.likes
                                       where a.announ_id == announ_id
                                 select a);
                int like_num = 0;
                int dislike_num = 0;
                int like_id = 0;
                int likes_final=0;
                foreach (var item in likes_arr)
                {
                    like_num = item.like_num;
                    dislike_num = item.disline_num;
                    like_id = item.id;
                }
                if (like_num == 0 && dislike_num == 0)
                {
                    iproby.Data_Model.like like = new iproby.Data_Model.like();
                    like.announ_id = announ_id;
                    like.like_num = like_num + 1;
                    like.disline_num = dislike_num;
                    db.likes.Add(like);
                    db.SaveChanges();
                    likes_final = like.like_num;
                }
                else {
                    var like = db.likes.Find(like_id);
                    if (like != null)
                    {
                        like.like_num = like_num+1;
                        db.SaveChanges();
                        likes_final = like.like_num;
                    }
                }
                return likes_final;
            //}
            //else
            //{
            //    var likes_arr = (from a in db.likes
            //                     where a.announ_id == announ_id
            //                     select a);
            //    int like_num = 0;
            //    int dislike_num = 0;
            //    int like_id = 0;
            //    int likes_final = 0;
            //    foreach (var item in likes_arr)
            //    {
            //        like_num = item.like_num;
            //        dislike_num = item.disline_num;
            //        like_id = item.id;
            //    }
            //    likes_final = like_num;
            //    return likes_final;
            //}
        }

        [HttpPost]
        public int AddDislike(int announ_id)
        {
            //if (Session["login"] != null)
            //{
                //string login = Session["login"].ToString();
                //var customer_id_arr = (from a in db.customers
                //                       where a.login == login
                //                       select a.customer_id);
                //int customer_id = 0;
                //foreach (int item in customer_id_arr)
                //{
                //    customer_id = item;
                //}
                var likes_arr = (from a in db.likes
                                 where a.announ_id == announ_id
                                 select a);
                int like_num = 0;
                int dislike_num = 0;
                int like_id = 0;
                int likes_final = 0;
                foreach (var item in likes_arr)
                {
                    like_num = item.like_num;
                    dislike_num = item.disline_num;
                    like_id = item.id;
                }
                if (like_num == 0 && dislike_num == 0)
                {
                    iproby.Data_Model.like like = new iproby.Data_Model.like();
                    like.announ_id = announ_id;
                    like.like_num = like_num + 1;
                    like.disline_num = dislike_num;
                    db.likes.Add(like);
                    db.SaveChanges();
                    likes_final = like.disline_num;
                }
                else
                {
                    var like = db.likes.Find(like_id);
                    if (like != null)
                    {
                        like.disline_num = dislike_num + 1;
                        db.SaveChanges();
                        likes_final = like.disline_num;
                    }
                }
                return likes_final;
            //}
            //else
            //{
            //    var likes_arr = (from a in db.likes
            //                     where a.announ_id == announ_id
            //                     select a);
            //    int like_num = 0;
            //    int dislike_num = 0;
            //    int like_id = 0;
            //    int likes_final = 0;
            //    foreach (var item in likes_arr)
            //    {
            //        like_num = item.like_num;
            //        dislike_num = item.disline_num;
            //        like_id = item.id;
            //    }
            //    likes_final = dislike_num;
            //    return likes_final;
            //}
        }

        public int GetVisits(int announ_id)
        {
            var visits_arr = (from a in db.visits
                             where a.announ_id == announ_id
                             select a.visits_num);
            int visits_num = 0;
            foreach (int item in visits_arr)
            {
                visits_num = item;
            }
            return visits_num;
        }

        public void AddVisit(int announ_id)
        {
            var visit_arr = (from a in db.visits
                                   where a.announ_id == announ_id
                                   select a);
            int visit_num = 0;
            int visit_id = 0;
            foreach (var item in visit_arr)
            {
                visit_num = item.visits_num.Value;
                visit_id = item.id;
            }
            if (visit_num == 0)
            {
                iproby.Data_Model.visit visit = new iproby.Data_Model.visit();
                visit.announ_id = announ_id;
                visit.visits_num = visit_num + 1;
                db.visits.Add(visit);
                db.SaveChanges();
            }
            else
            {
                var visit = db.visits.Find(visit_id);
                if (visit != null)
                {
                    visit.visits_num = visit_num + 1;
                    db.SaveChanges();
                }
            }
        }

        private static string TruncateAtWord(string input, int length)
        {
            if (input == null || input.Length < length)
                return input;
            int iNextSpace = input.LastIndexOf(" ", length);
            return string.Format("{0}...", input.Substring(0, (iNextSpace > 0) ? iNextSpace : length).Trim());
        }

        private string SkipHtml(string html)
        {
            Regex regex = new Regex("\\<[^\\>]*\\>");
            string clear_text = regex.Replace(html, String.Empty);
            clear_text = clear_text.Replace("&nbsp;", " ").Replace("&laquo;", "«").Replace("&raquo;", "»");

            return clear_text;
        }

    }
}

