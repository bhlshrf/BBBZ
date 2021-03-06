﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BBBZ.Controllers
{
    public class ErrorController : BaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base._checkforuserlockedFLAG = false;
            base.OnActionExecuting(filterContext);
        }

        public ActionResult Index(int? id)
        {
            ViewBag.PreviousUrl = Session["PreviousUrl"];
            if (id == null)
            {
                ViewBag.Code = Session["errorCode"];
                ViewBag.Message = Session["errorMessage"];
            }
            else
            {
                ViewBag.Code = id;
                switch (id)
                {
                    case 400: ViewBag.Message = "bad request"; break;
                    case 401: ViewBag.Message = "you not allowed to see this"; break;
                    case 404: ViewBag.Message = "resource not found"; break;
                    case 500: ViewBag.Message = "internal server error"; break;
                    default: ViewBag.Message = "(" + ((HttpStatusCode)id).ToString() +" )unkowen error has been occure"; break;
                }
            }
            return View();
        }
	}
}