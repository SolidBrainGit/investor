﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Investor.Service.Interfaces;
using Investor.Model;
using Investor.Repository;
using Investor.Repository.Interfaces;

namespace Investor.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPostService _postService;
        private readonly IPostRepository _db;
        private readonly ICategoryService _categoryService;

        public HomeController(IPostService postService, ICategoryService categoryService, IPostRepository db)
        {
            _postService = postService;
            _categoryService = categoryService;
            _db = db;
        }

        public IActionResult Index()
        {
            IList<Category> categories = _categoryService.GetAllCategoriesAsync().Result.ToList();
            ViewBag.LatestPost = _postService.GetLatestPostsAsync(20).Result.ToList();
            ViewBag.Categories = categories;
            ViewBag.Politics = _postService.GetAllByCategoryNameAsync(categories[0].Name, true, 8).Result.ToList();
            ViewBag.Technologies = _postService.GetAllByCategoryNameAsync(categories[4].Name, true, 8).Result.ToList();
            ViewBag.Culture  = _postService.GetAllByCategoryNameAsync(categories[2].Name, true, 4).Result.ToList();
            ViewBag.Economy  = _postService.GetAllByCategoryNameAsync(categories[3].Name, true, 4).Result.ToList();
            return View();

        }

    }
}
