﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using RouteMaster.Models.EFModels;
using RouteMaster.Models.Infra.EFRepositories;
using RouteMaster.Models.Infra.Extensions;
using RouteMaster.Models.Interfaces;
using RouteMaster.Models.Services;
using RouteMaster.Models.ViewModels;
using WebGrease.Css.Extensions;

namespace RouteMaster.Controllers
{
    public class PackageToursController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: PackageTours
        public ActionResult Index()
        {

           
            IPackageTourRepository repo =new PackageTourEFRepository();
            PackageTourService service = new PackageTourService(repo);


            return View(service.Search().ToList().Select(x => x.ToIndexVM()));

        }



        


        // GET: PackageTours/Create


        public ActionResult Create()
        {
            ViewBag.Attractions=db.Attractions.ToList().Select(x=>x.ToAttractionListIndexDto().ToAttractionListIndexVM());
            ViewBag.Activities = db.Activities.ToList().Select(x=>x.ToIndexDto().ToIndexVM());
            ViewBag.ExtraServices=db.ExtraServices.ToList().Select(x=>x.ToIndexDto().ToIndexVM());          


            PrepareCouponDataSource(null);
            return View();
        }






        // POST: PackageTours/Create
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]

        public ActionResult Create(PackageTourCreateVM vm) 
		{
            IPackageTourRepository repo = new PackageTourEFRepository();
            PackageTourService service = new PackageTourService(repo);



            //根據接收到的活動Id獲取完整的活動對象           
            List<Activity> selectedActivities = new List<Activity>();
            foreach (var activityId in vm.Activities.Select(a => a.Id))
            {
                var activity = db.Activities.Find(activityId);
                selectedActivities.Add(activity);
            }
            //將完整的活動物件添加到Activities列表當中
            vm.Activities = selectedActivities.Select(a => a.ToIndexDto().ToIndexVM()).ToList();
         


            List<ExtraService> selectedExtraService=new List<ExtraService>();
            foreach(var ectraServiceId in vm.ExtraServices.Select(e => e.Id))
            {
                var extraService = db.ExtraServices.Find(ectraServiceId);
                selectedExtraService.Add(extraService);
            }
            vm.ExtraServices=selectedExtraService.Select(e=>e.ToIndexDto().ToIndexVM()).ToList();   




            List<Attraction> selectedAttractions=new List<Attraction>();
            foreach(var attractionId in vm.Attractions.Select(a => a.Id))
            {
                var attraction = db.Attractions.Find(attractionId);
                selectedAttractions.Add(attraction);
            }
            vm.Attractions=selectedAttractions.Select(a=>a.ToAttractionListIndexDto().ToAttractionListIndexVM()).ToList();






            if (ModelState.IsValid == false)
            {
                PrepareCouponDataSource(vm.CouponId);
                return View(vm);
            }

            if (ModelState.IsValid)
            {           
                service.Create(vm.ToCreateDto());
                return RedirectToAction("Index");
            }

            PrepareCouponDataSource(vm.CouponId);


            return View("Index");
        }





        // GET: PackageTours/Edit/5
        public ActionResult Edit(int id)
        {
            IPackageTourRepository repo = new PackageTourEFRepository();
            PackageTourService service = new PackageTourService(repo);


            var packageTour= service.GetPackageTourById(id);
            PrepareCouponDataSource(packageTour.CouponId);
            
            return View(packageTour.ToEditDto().ToEditVM());
        }




        // POST: PackageTours/Edit/5
        // 若要避免過量張貼攻擊，請啟用您要繫結的特定屬性。
        // 如需詳細資料，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Description,Status,CouponId")] PackageTour packageTour)
        {
            if (ModelState.IsValid)
            {
                db.Entry(packageTour).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CouponId = new SelectList(db.Coupons, "Id", "Discount", packageTour.CouponId);
            return View(packageTour);
        }

        // GET: PackageTours/Delete/5
        public ActionResult Delete(int id)
        {
            IPackageTourRepository repo = new PackageTourEFRepository();
            PackageTourService service = new PackageTourService(repo);

            return View(service.GetPackageTourById(id).ToIndexDto().ToIndexVM());
            
        }

        // POST: PackageTours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            IPackageTourRepository repo = new PackageTourEFRepository();
            PackageTourService service = new PackageTourService(repo);

            service.Delete(id);

            return RedirectToAction("Index");
        }




        // GET: PackageTours/Details/5
        public ActionResult Details(int id)
        {
            IPackageTourRepository repo = new PackageTourEFRepository();
            PackageTourService service = new PackageTourService(repo);

            return View(service.GetPackageTourById(id).ToIndexDto().ToIndexVM());
        }



        public ActionResult ActivitiesList()
        {
            var model=db.Activities.ToList().Select(x=>x.ToIndexDto().ToIndexVM());
            //取得模型

            return this.PartialView("_ActivitiesListPartial", model);
        }

        public ActionResult ExtraServicesList()
        {
            var model = db.ExtraServices.ToList().Select(x => x.ToIndexDto().ToIndexVM());
            //取得模型

            return this.PartialView("_ExtraServicesListPartial", model);
        }


        public ActionResult AttractionsList()
        {
            var model = db.Attractions.ToList();                
             
            //取得模型

            return this.PartialView("_AttractionsListPartial", model);
        }






        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }



        private void PrepareCouponDataSource(int? couponId)
        {
            var coupons = db.Coupons.ToList().Prepend(new Coupon() {Discount=1 });
            ViewBag.CouponId = new SelectList(coupons, "Id", "Discount", couponId);
        }
    }
}
