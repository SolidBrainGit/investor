﻿using Investor.Model;
using Investor.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Investor.Web.Controllers
{
    public class PostController : Controller
    {
        private readonly INewsService _postService;
        private readonly ITagService _tagService;
        private IHttpContextAccessor _accessor;
        public PostController(INewsService postService, ITagService tagService, IHttpContextAccessor accessor)
        {
            _postService = postService;
            _tagService = tagService;
            _accessor = accessor;
        }
        public IActionResult Index(int id)
        {
            ViewBag.PathBase = Request.Host.Value;
            var post = _postService.GetNewsByIdAsync(id).Result;

            if (!(post?.IsPublished) ?? true)
            {
                Response.StatusCode = 403;
                return StatusCode(Response.StatusCode);
            }
            ViewBag.Post = post;
            ViewBag.LatestPosts = _postService.GetLatestNewsAsync(10).Result?.ToList();
            ViewBag.ImportantPosts = _postService.GetImportantNewsAsync(10).Result?.ToList();
            ViewBag.Tags = _postService.GetAllTagsByNewsIdAsync(id).Result?.ToList();
            ViewBag.PopularTags = _tagService.GetPopularTagsAsync(5).Result?.ToList();
            var ip = IPAddress.Loopback;
            NetworkCredential cred = CredentialCache.DefaultNetworkCredentials;
            WebClient wc = new WebClient();
            wc.Credentials = cred;
            string externalip = wc.DownloadString("http://icanhazip.com");
            var ip2 = _accessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            ViewBag.IP = externalip;
            return View("Index");
        }

        public IActionResult LastNews()
        {
            IEnumerable<PostPreview> lastNews = _postService.GetLatestNewsAsync(10).Result.ToList();
            return View(lastNews);
        }


    }
}
