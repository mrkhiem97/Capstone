﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MobileSurveillanceWebApplication.Controllers
{
    public class ProfileController : Controller
    {
        //
        // GET: /Profile/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UserProfile()
        {
            return View();
        }

    }
}
