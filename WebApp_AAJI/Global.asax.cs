using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Globalization;
using System.Threading;
using WebApp_AAJI.App_Start;

namespace WebApp_AAJI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
	     ModelBinders.Binders.Add(typeof(DateTime?), new MyDateTimeModelBinder());
        }

	 //protected void Application_BeginRequest(Object sender, EventArgs e)
	 //{
	 //	CultureInfo newCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
	 //	newCulture.DateTimeFormat.ShortDatePattern = "dd-MM-yyyy";
	 //	newCulture.DateTimeFormat.DateSeparator = "-";
	 //	Thread.CurrentThread.CurrentCulture = newCulture;
	 //}
    }
}
