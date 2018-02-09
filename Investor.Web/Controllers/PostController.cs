﻿using Investor.Model;
using Investor.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Investor.Service.Utils;
using Investor.Web.Filters;

namespace Investor.Web.Controllers
{
    public class PostController : Controller
    {
        private readonly INewsService _postService;
        private readonly ITagService _tagService;

        public PostController(INewsService postService, ITagService tagService, IpService ipService)
        {
            _postService = postService;
            _tagService = tagService;
        }

        [ServiceFilter(typeof(HitCount))]
        public IActionResult Index(string postUrl,int id)
        {
            ViewBag.PathBase = Request.Host.Value;

            News post = _postService.GetNewsByIdAsync(id).Result;

            if (post == null || !post.IsPublished || post.Url != postUrl)
            {
                Response.StatusCode = 404;
                return StatusCode(Response.StatusCode);
            }

            ViewBag.Post = post;
            ViewBag.LatestPosts = _postService.GetLatestNewsAsync(10).Result?.ToList();
            ViewBag.ImportantPosts = _postService.GetImportantNewsAsync(10).Result?.ToList();
            ViewBag.Tags = _postService.GetAllTagsByNewsIdAsync(id).Result?.ToList();
            ViewBag.PopularTags = _tagService.GetPopularTagsAsync(5).Result?.ToList();

            return View("Index");
        }

        [Route("/unpublished/{postUrl}-{id}")]
        public IActionResult PostPreview(string postUrl,int id)
        {
            News post = _postService.GetNewsByIdAsync(id).Result;
            if (!HttpContext.User.IsInRole("admin") || post == null || post.Url != postUrl || post.IsPublished)
                return new StatusCodeResult(404);
            ViewBag.PathBase = Request.Host.Value;
            ViewBag.Post = post;
            ViewBag.LatestPosts = _postService.GetLatestNewsAsync(10).Result?.ToList();
            ViewBag.ImportantPosts = _postService.GetImportantNewsAsync(10).Result?.ToList();
            ViewBag.Tags = _postService.GetAllTagsByNewsIdAsync(id).Result?.ToList();
            ViewBag.PopularTags = _tagService.GetPopularTagsAsync(5).Result?.ToList();

            return View("UnpublishedPost");
        }

        public IActionResult LastNews()
        {
            IEnumerable<PostPreview> lastNews = _postService.GetLatestNewsAsync(10).Result.ToList();
            return View(lastNews);
        }

        

        

    }
}


