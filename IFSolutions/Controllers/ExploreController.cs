﻿using IFSolutions.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IFSolutions.Controllers
{
    [Authorize]
    public class ExploreController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();

        // GET: Explore
        [AllowAnonymous]
        public ActionResult Index(int? campusID, bool solved = false)
        {
            ViewBag.CampiList = new SelectList(db.Campus.OrderBy(m => m.Description), "CampusID", "Description");

            if (solved)
            {
                ViewBag.DivClass = "panel panel-success";
                ViewBag.Glyphicon = "glyphicon glyphicon-ok-circle";
            }
            else
            {
                ViewBag.DivClass = "panel panel-danger";
                ViewBag.Glyphicon = "glyphicon glyphicon-remove-circle";
            }

            if (campusID.HasValue)
            {
                var listPetitions = db.Petitions.Where(m => m.CampusID == campusID && m.Solved == solved)
                    .OrderByDescending(m => m.Signatures.Count).Take(20);

                return View(listPetitions.ToList());
            }

            var listAllPetitions = db.Petitions.OrderByDescending(m => m.Signatures.Count).Where(m => m.Solved == solved).Take(20);

            return View(listAllPetitions.ToList());
        }

        [AllowAnonymous]
        public ActionResult Solved()
        {
            return View();
        }

        // GET: Explore/Details/5
        [AllowAnonymous]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            Petition petition = db.Petitions.Find(id);

            if (petition == null)
            {
                return RedirectToAction("Index");
            }

            string userId = User.Identity.GetUserId();

            var checkNumSignature = db.Signatures
                .Where(m => m.UserId.Equals(userId, StringComparison.CurrentCultureIgnoreCase)
                && m.PetitionID == id).Count();
            
            if (checkNumSignature == 0)
            {
                ViewBag.UserSigned = false;
            }
            else
            {
                ViewBag.UserSigned = true;
            }

            return View(petition);
        }

        [HttpPost]
        public ActionResult Details(string commentContent, int petitionID)
        {
            if (String.IsNullOrEmpty(commentContent))
            {
                return RedirectToAction("Index");
            }

            Comment comment = new Comment()
            {
                Content = commentContent,
                PetitionID = petitionID,
                DateTime = DateTime.Now,
                UserId = User.Identity.GetUserId()
            };

            db.Comments.Add(comment);
            db.SaveChanges();

            return RedirectToAction("Details", "Explore", petitionID);
        }

        public ActionResult SignPetition(int petitionID)
        {
            Signature signature = new Signature()
            {
                PetitionID = petitionID,
                UserId = User.Identity.GetUserId()
            };

            db.Signatures.Add(signature);
            db.SaveChanges();

            return RedirectToAction("Details/" + petitionID, "Explore");
        }

        public ActionResult UnsignPetition(int petitionID)
        {
            string userID = User.Identity.GetUserId();

            var signature = db.Signatures
                .Where(m => m.PetitionID == petitionID && m.UserId.Equals(userID, StringComparison.CurrentCultureIgnoreCase))
                .FirstOrDefault();

            if (signature == null)
            {
                return RedirectToAction("Index");
            }

            db.Signatures.Remove(signature);
            db.SaveChanges();

            return RedirectToAction("Details/" + petitionID, "Explore");
        }

        public ActionResult DeleteComment(int commentID, int petitionID)
        {
            string userID = User.Identity.GetUserId();

            var comment = db.Comments.Find(commentID);

            if (comment.UserId.Equals(userID, StringComparison.CurrentCultureIgnoreCase))
            {
                db.Comments.Remove(comment);
                db.SaveChanges();
            }

            return RedirectToAction("Details/" + petitionID, "Explore");
        }
    }
}