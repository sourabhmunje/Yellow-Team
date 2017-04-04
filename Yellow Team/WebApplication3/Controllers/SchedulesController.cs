﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class SchedulesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Schedules
        
        public ActionResult Index()
        {
            DateTime d1 = System.DateTime.Now;
            d1 = d1.AddDays(-1);
            string previousday = d1.ToShortDateString();
            var Curdate = System.DateTime.Now.ToShortDateString();
            var count = db.Schedules.Where(c=>c.created==Curdate).Count();
            if(count == 0) { 
            LoadSchedule(previousday);
            }
            var schedules = db.Schedules.Where(s => s.created == Curdate).OrderBy(s=>s.Priority);
            return View(schedules.ToList());
        }

        public ActionResult Finalize()
        {
            var Curdate = System.DateTime.Now.ToShortDateString();
            var schedules = db.Schedules.Where(sc => sc.created == Curdate);
            foreach(Schedule sc in schedules)
            {
                sc.ScheduledCheckIn = sc.EstimatedCheckin;
                sc.ScheduledCheckout = sc.EstimatedCheckout;
            }
            db.SaveChanges();
            return RedirectToAction("Index", "Schedules");
        }

        public void LoadSchedule(string pd)
        {
            string previousday = pd;
            
            var scheduleid = db.Schedules.Max(sid => sid.Id);
            scheduleid = ++scheduleid;
            var previousschedule = db.Schedules.Where(ps => ps.created==previousday);
            
            foreach (Schedule ps in previousschedule){
                var NewSchedule = new Schedule();
                NewSchedule.Id = scheduleid;
                NewSchedule.Length = ps.Length;
                NewSchedule.Parent = false;
                NewSchedule.Priority = ps.Priority;
                NewSchedule.room = ps.room;
                NewSchedule.RoomId = ps.RoomId;
                NewSchedule.CheckIn = "";
                NewSchedule.CheckOut = "";
                NewSchedule.completed = 0;
                NewSchedule.created = System.DateTime.Now.ToShortDateString();
                
                scheduleid = ++scheduleid;
                db.Schedules.Add(NewSchedule);
                
            }
            db.SaveChanges();
            updateschedule();
        }

        public void updateschedule()
        {
            var Curdate = System.DateTime.Now.ToShortDateString();
            var schedules = db.Schedules.Where(s => s.created == Curdate);
            foreach(Schedule s in schedules)
            {
                TimeScheduling(s);
                db.Entry(s).State = EntityState.Modified;
            }
            db.SaveChanges();
        }

        public ActionResult CheckIn(int? id)
        {

            var schedule = db.Schedules.Single(s => s.Id == id);
            schedule.CheckIn = System.DateTime.Now.ToShortTimeString();
            db.Entry(schedule).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult CheckOut(int? id)
        {

            var schedule = db.Schedules.Single(s => s.Id == id);
            schedule.CheckOut = System.DateTime.Now.ToShortTimeString();
            schedule.completed = 1;
            DateTime temp = Convert.ToDateTime(schedule.ScheduledCheckout);
            DateTime temp1 = Convert.ToDateTime(schedule.CheckOut);
            TimeSpan span = temp1.Subtract(temp);
            schedule.late = span.Minutes;
            temp = temp.AddMinutes(Convert.ToInt32(schedule.Length));
            schedule.EstimatedCheckout = temp.ToShortTimeString();
            db.Entry(schedule).State = EntityState.Modified;
            db.SaveChanges();
            var updateschedule = db.Schedules.Where(up => up.Priority > schedule.Priority);
            foreach(Schedule up in updateschedule)
            {
                TimeScheduling(up);
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Clear(int? id)
        {

            var schedule = db.Schedules.Single(s => s.Id == id);
            schedule.CheckIn = "";
            schedule.CheckOut = "";
            schedule.completed = 0;
            db.Entry(schedule).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Moveup(int? id)
        {
            var schedule = db.Schedules.Single(s => s.Id == id);
            var schedulepriority = db.Schedules.Single(sp => sp.Priority == schedule.Priority - 1);
            if (schedule.Priority != 1 & schedulepriority.completed==0) { 
            
            schedule.Priority = schedulepriority.Priority + schedule.Priority;
            schedulepriority.Priority = schedule.Priority - schedulepriority.Priority;
            schedule.Priority = schedule.Priority - schedulepriority.Priority;
            TimeScheduling(schedule);
            db.Entry(schedule).State = EntityState.Modified;
            db.Entry(schedulepriority).State = EntityState.Modified;
            db.SaveChanges();
            
            }
            TimeScheduling(schedulepriority);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult MoveDown(int? id)
        {
            var schedule = db.Schedules.Single(s => s.Id == id);
            var maxpriority = db.Schedules.Max(mp => mp.Priority);
            var schedulepriority = db.Schedules.Single(sp => sp.Priority == schedule.Priority + 1);
            if (schedule.Priority != maxpriority & schedulepriority.completed==0)
            {
               
                schedule.Priority = schedulepriority.Priority + schedule.Priority;
                schedulepriority.Priority = schedule.Priority - schedulepriority.Priority;
                schedule.Priority = schedule.Priority - schedulepriority.Priority;
                
                TimeScheduling(schedulepriority);
                db.Entry(schedule).State = EntityState.Modified;
                db.Entry(schedulepriority).State = EntityState.Modified;
                db.SaveChanges();
            }
            TimeScheduling(schedule);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        // GET: Schedules/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Schedule schedule = db.Schedules.Find(id);
            if (schedule == null)
            {
                return HttpNotFound();
            }
            return View(schedule);
        }

        // GET: Schedules/Create
        public ActionResult Create()
        {
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Room_Number");
            var Curdate = System.DateTime.Now.ToShortDateString();
            var count = db.Schedules.Where(c => c.created == Curdate).Count();
            if(count==0)
            {
                priority = 1;
            }
            else
            { 
            var schedulepriority = db.Schedules.Where(s=>s.created==Curdate).Max(s=>s.Priority);
                priority = schedulepriority + 1;
            }
            return View();
        }

        // POST: Schedules/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Length,Parent,EstimatedCheckin,EstimatedCheckout,CheckIn,CheckOut,RoomId")] Schedule schedule)
        {
            
            schedule.created = System.DateTime.Now.ToShortDateString();
            schedule.completed = 0;
            schedule.Priority = priority;
            schedule.late = 0;
            TimeScheduling(schedule);
            priority = 0;
            if (ModelState.IsValid)
            {
                db.Schedules.Add(schedule);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Room_Number", schedule.RoomId);
            return View(schedule);
        }

        public void TimeScheduling(Schedule schedule)
        {
            if (schedule.completed == 0)
            {
                if (priority == 1)
                {
                    schedule.EstimatedCheckin = "9:00 AM";
                }
                else
                {
                    var tempschedule = db.Schedules.Single(ts => ts.Priority == schedule.Priority - 1);
                    if (tempschedule.CheckOut == null)
                    {
                        schedule.EstimatedCheckin = tempschedule.EstimatedCheckout;
                    }
                    else
                    {
                        schedule.EstimatedCheckin = tempschedule.CheckOut;
                    }
                }
                DateTime temp = Convert.ToDateTime(schedule.EstimatedCheckin);
                temp = temp.AddMinutes(Convert.ToInt32(schedule.Length));
                schedule.EstimatedCheckout = temp.ToShortTimeString();
                DateTime temp1 = Convert.ToDateTime(schedule.ScheduledCheckout);
                DateTime temp2 = Convert.ToDateTime(schedule.EstimatedCheckout);
                TimeSpan span = temp2.Subtract(temp1);
                schedule.late = span.Minutes;
                //db.SaveChanges();
            }
        }

        // GET: Schedules/Edit/5
        public static int priority;
        public static int completed;
        public static string created;
        public static int late;
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Schedule schedule = db.Schedules.Find(id);
            if (schedule == null)
            {
                return HttpNotFound();
            }
            priority = schedule.Priority;
            completed = schedule.completed;
            created = schedule.created;
            late = schedule.late;
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Room_Number", schedule.RoomId);
            return View(schedule);
        }

        // POST: Schedules/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Length,Parent,EstimatedCheckin,EstimatedCheckout,CheckIn,CheckOut,RoomId")] Schedule schedule)
        {
            if (ModelState.IsValid)
            {
                schedule.Priority = priority;
                schedule.completed = completed;
                schedule.created = created;
                schedule.late = late;
                db.Entry(schedule).State = EntityState.Modified;
                db.SaveChanges();
                priority = completed = late = 0;
                created = "";
                return RedirectToAction("Index");
            }
            ViewBag.RoomId = new SelectList(db.Rooms, "Id", "Room_Number", schedule.RoomId);
            return View(schedule);
        }

        // GET: Schedules/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Schedule schedule = db.Schedules.Find(id);
            if (schedule == null)
            {
                return HttpNotFound();
            }
            return View(schedule);
        }

        // POST: Schedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Schedule schedule = db.Schedules.Find(id);
            db.Schedules.Remove(schedule);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
