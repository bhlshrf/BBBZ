﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BBBZ.Models;

namespace BBBZ.Controllers
{
    public class MenuItemController : BaseController
    {
        // GET: /MenuItem/
        public ActionResult Index(int? id)
        {
            if (id != null)
            {
                ViewBag.ID = id;
                return View(db.Menus
                    .Include(x => x.MenuType)
                    .Where(x => x.MenuType != null && x.MenuType.ID == id)
                    .ToList()
                    .FillWithChildren()
                    .ConvertToViewModel());
            }
            return View(db.Menus
                .Include(x => x.MenuType)
                .Where(x => x.Parent == null)
                .ToList()
                .FillWithChildren()
                .ConvertToViewModel());
        }

        // GET: /MenuItem/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Menu menu = db.Menus.Include(x => x.Access).Include(x => x.MenuType).Include(x => x.Parent).SingleOrDefault(x => x.ID == id);
            if (menu == null)
            {
                return HttpNotFound();
            }
            return View(menu);
        }

        // GET: /MenuItem/Create
        public ActionResult Create(int? selectedMenuTypeID, string itemtype = "")
        {
            MenuItemViewModel model = new MenuItemViewModel();
            model.ItemType = itemtype;
            model.selectedMenuTypeID = selectedMenuTypeID;
            model.AllMenuTypes = db.MenuTypes.ToList();
            
            if (selectedMenuTypeID != null)
            {
                model.TheMenuType = db.MenuTypes.SingleOrDefault(x => x.ID == selectedMenuTypeID);
                if (model.TheMenuType != null)
                {
                    model.AllMenus = Extenisons.GetAllMenuItems(db.Menus.Where(x => x.MenuType != null && x.MenuType.ID == model.TheMenuType.ID).ToList());
                }
            }
            else
                model.AllMenus = Extenisons.GetAllMenuItems();

            return View(model);
        }

        // POST: /MenuItem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MenuItemViewModel menu)
        {
            if (ModelState.IsValid)
            {
                Menu m = new Menu()
                {
                    Type = menu.ItemType,

                    Title = menu.TheMenu.Title,
                    Alias = string.IsNullOrEmpty(menu.TheMenu.Alias) ? (menu.TheMenu.Title.ToLower().Replace(" ", "")) : menu.TheMenu.Alias.ToLower().Replace(" ", ""),
                    Note = menu.TheMenu.Note,
                    Published = menu.TheMenu.Published,
                    OpenInSameWindow = menu.TheMenu.OpenInSameWindow,
                    Langauge = menu.TheMenu.Langauge,

                    CategoryID = menu.selectedCategoryID,
                    ContentID = menu.selectedContentID,
                    Url = menu.TheMenu.Url,
                };

                if (menu.selectedAccessID != null)
                    m.Access = db.ViewLevels.SingleOrDefault(x => x.ID == menu.selectedAccessID);

                if (menu.selectedParentID != null)
                    m.Parent = db.Menus.SingleOrDefault(x => x.ID == menu.selectedParentID);
                else if (menu.selectedMenuTypeID != null)
                    m.MenuType = db.MenuTypes.SingleOrDefault(x => x.ID == menu.selectedMenuTypeID);

                db.Menus.Add(m);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(menu);
        }

        // GET: /MenuItem/Edit/5
        public ActionResult Edit(int? id, int? selectedMenuTypeID, string itemtype = "")
        {
            if (id == null)
                return BadRequest();
            Menu menu = db.Menus.Include(x=>x.Access).Include(x => x.Parent).Include(x => x.MenuType).SingleOrDefault(x => x.ID == id);
            if (menu == null)
                return HttpNotFound();

            MenuItemViewModel model = new MenuItemViewModel();
            model.ItemType = string.IsNullOrEmpty(itemtype) ? menu.Type : itemtype;
            model.AllMenuTypes = db.MenuTypes.ToList();
            model.TheMenu = menu;
            model.selectedAccessID = menu.Access == null ? null : (int?)menu.Access.ID;
            model.selectedParentID = menu.Parent == null ? null : (int?)menu.Parent.ID;
            model.TheMenuType = menu.MenuType;

            model.selectedContentID = menu.ContentID;
            model.selectedCategoryID = menu.CategoryID;

            if (selectedMenuTypeID != null && model.TheMenuType != null && model.TheMenuType.ID != selectedMenuTypeID)
            {
                model.selectedMenuTypeID = selectedMenuTypeID;
                model.TheMenuType = db.MenuTypes.SingleOrDefault(x => x.ID == selectedMenuTypeID);
            }

            if (model.TheMenuType != null)
            {
                model.AllMenus = Extenisons.GetAllMenuItems(
                    db.Menus.Where(x => x.MenuType != null && x.MenuType.ID == model.TheMenuType.ID).ToList(),
                    (int)id);
                model.selectedMenuTypeID = model.TheMenuType.ID;
            }
            else
            {
                model.selectedMenuTypeID = null;
                model.AllMenus = Extenisons.GetAllMenuItems(null, (int)id);
            }

            return View(model);
        }

        // POST: /MenuItem/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MenuItemViewModel menu)
        {
            if (ModelState.IsValid)
            {
                if (menu.id == menu.selectedParentID)
                    ModelState.AddModelError("", "Parent can't be same item");
                else
                {
                    Menu m = db.Menus
                        .Include(x => x.Access)
                        .Include(x => x.MenuType)
                        .Include(x => x.Parent)
                        .SingleOrDefault(x => x.ID == menu.id);
                    if (m != null)
                    {
                        m.Type = menu.ItemType;

                        m.Title = menu.TheMenu.Title;
                        m.Alias = string.IsNullOrEmpty(menu.TheMenu.Alias) ?
                            menu.TheMenu.Title.ToLower().Replace(" ", "") :
                            menu.TheMenu.Alias.ToLower().Replace(" ", "");
                        m.Note = menu.TheMenu.Note;
                        m.Published = menu.TheMenu.Published;
                        m.OpenInSameWindow = menu.TheMenu.OpenInSameWindow;
                        m.Langauge = menu.TheMenu.Langauge;

                        m.CategoryID = menu.selectedCategoryID;
                        m.ContentID = menu.selectedContentID;
                        m.Url = menu.TheMenu.Url;

                        if (menu.selectedAccessID != null)
                            m.Access = db.ViewLevels.SingleOrDefault(x => x.ID == menu.selectedAccessID);

                        if (m.Parent != null && menu.selectedParentID == null)
                            m.Parent = null;
                        else if (menu.selectedParentID != null)
                        {
                            if (m.Parent == null || (m.Parent != null && m.Parent.ID != menu.selectedParentID))
                                m.Parent = db.Menus.SingleOrDefault(x => x.ID == menu.selectedParentID);
                        }

                        if (m.Parent == null && menu.selectedMenuTypeID != null)
                            if (m.MenuType == null || (m.MenuType != null && m.MenuType.ID != menu.selectedMenuTypeID))
                                m.MenuType = db.MenuTypes.SingleOrDefault(x => x.ID == menu.selectedMenuTypeID);

                        db.Entry(m).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
            }
            return View(menu);
        }

        // GET: /MenuItem/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Menu menu = db.Menus.Find(id);
            if (menu == null)
            {
                return HttpNotFound();
            }
            return View(menu);
        }

        // POST: /MenuItem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Menu menu = db.Menus.Find(id);
            db.Menus.Remove(menu);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}
