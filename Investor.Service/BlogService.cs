﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Investor.Entity;
using Investor.Model;
using Investor.Repository.Interfaces;
using Investor.Service.Interfaces;
using Investor.Model.Views;

namespace Investor.Service
{
    public class BlogService : IBlogService
    {
        private readonly IBlogRepository _blogRepository;
        private readonly IPostRepository _postRepository;
        private readonly ITagService _tagService;
        private readonly ICategoryService _categoryService;


        public BlogService(IBlogRepository blogRepository, IPostRepository postRepository, ITagService tagService, ICategoryService categoryService)
        {
            _blogRepository = blogRepository;
            _postRepository = postRepository;
            _tagService = tagService;
            _categoryService = categoryService;
        }


        public async Task<Blog> AddBlogAsync(BlogViewModel map)
        {
            map.Category = await _categoryService.GetCategoryByUrlAsync("blog");
            var response = await _postRepository.AddPostAsync<BlogEntity>(Mapper.Map<BlogViewModel, BlogEntity>(map));
            await AddTagsToBlogAsync(response.PostId, map.Tags?.Select(c => c.Name));
            return Mapper.Map<BlogEntity, Blog>(response);
        }

        public async Task AddTagsToBlogAsync(int postId, IEnumerable<string> tags) // TODO rename method
        {
            tags = tags ?? new List<string>();
            List<TagEntity> postTags = await _postRepository.GetAllTagsByPostIdAsync(postId);
            IEnumerable<string> enumerable = tags as IList<string> ?? tags.ToList();
            foreach (var t in enumerable)
            {
                Tag tag = await _tagService.GetTagByNameAsync(t) ?? await _tagService.AddTagAsync(new Tag { Name = t });
                if (!postTags.Select(s => s.Name).Contains(t))
                {
                    await _postRepository.AddTagToPostAsync(postId, Mapper.Map<Tag, TagEntity>(tag));
                }
            }
            var tagsToRemove = postTags?.Where(pt => !tags.Contains(pt.Name)).ToList() ?? new List<TagEntity>();
            foreach (var pt in tagsToRemove)
            {
                await _postRepository.RemoveTagFromPostAsync(postId, pt);
            }

        }

        public async Task<Tag> AddTagToBlogAsync(int postId, Tag tag)
        {
            TagEntity tagEntity = await _postRepository.AddTagToPostAsync(postId, Mapper.Map<Tag, TagEntity>(tag));
            return Mapper.Map<TagEntity, Tag>(tagEntity);
        }

        public async Task<IEnumerable<T>> GetAllBlogsByTagNameAsync<T>(string tagName)
        {
            IEnumerable<BlogEntity> blogs = await _postRepository.GetAllPostsByTagNameAsync<BlogEntity>(tagName);
            return blogs.Select(Mapper.Map<BlogEntity, T>);
        }

        public async Task<List<Tag>> GetAllTagsByBlogIdAsync(int id)
        {
            List<TagEntity> tags = await _postRepository.GetAllTagsByPostIdAsync(id) ?? new List<TagEntity>();
            return tags.Select(Mapper.Map<TagEntity, Tag>).ToList();
        }

        public async Task<IEnumerable<Blog>> GetBlogsBasedOnIdCollectionAsync(IEnumerable<int> postIds)
        {
            IEnumerable<BlogEntity> blogs = await _postRepository.GetPostsBasedOnIdCollectionAsync<BlogEntity>(postIds);
            return blogs.Select(Mapper.Map<BlogEntity, Blog>);
        }

        public async Task<IEnumerable<BlogPreview>> GetBlogsByUserIdAsync(string userId, bool? isPublished = null)
        {
            IEnumerable<BlogEntity> blogs = await _blogRepository.GetBlogsByUserIdAsync(userId, isPublished);
            return blogs.Select(Mapper.Map<BlogEntity, BlogPreview>);
        }

        public async Task<IEnumerable<BlogPreview>> GetPublishedBlogsByUserIdAsync(string userId)
        {
            IEnumerable<BlogEntity> blogs = (await _blogRepository.GetBlogsByUserIdAsync(userId)).Where(c => c.IsPublished ?? false);
            return blogs.Select(Mapper.Map<BlogEntity, BlogPreview>);
        }

        public async Task<IEnumerable<T>> GetLatestBlogsAsync<T>(int limit = 10)
        {
            IEnumerable<BlogEntity> blogs = await _blogRepository.GetLatestBlogsAsync(limit);
            return blogs.Select(Mapper.Map<BlogEntity, T>);
        }

        public async Task<IEnumerable<BlogPreview>> GetPopularBlogsAsync(int limit = 3)
        {
            IEnumerable<BlogEntity> blogs = await _blogRepository.GetPopularBlogsAsync(limit);
            return blogs.Select(Mapper.Map<BlogEntity, BlogPreview>);
        }

        public async Task<T> GetBlogByIdAsync<T>(int id)
        {
            BlogEntity blog = await _postRepository.GetPostByIdAsync<BlogEntity>(id);
            return Mapper.Map<BlogEntity, T>(blog);
        }

        public async Task RemoveBlogAsync(int id)
        {
            await _postRepository.RemovePostAsync(id);
        }

        public async Task RemoveBlogAsync(IEnumerable<int> id)
        {
            await _postRepository.RemovePostAsync(id);
        }

        public async Task<Blog> UpdateBlogAsync(BlogViewModel post)
        {
            BlogEntity blog = await _postRepository.UpdatePostAsync<BlogEntity>(Mapper.Map<BlogViewModel, BlogEntity>(post));
            await AddTagsToBlogAsync(blog.PostId, post.Tags?.Select(c => c.Name));
            return Mapper.Map<BlogEntity, Blog>(blog);
        }

        public async Task UpdateBlogAsync<T>(IEnumerable<T> post)
        {
            await _postRepository.UpdatePostAsync<BlogEntity>(post.Select(Mapper.Map<T, BlogEntity>));
        }

        public async Task<IEnumerable<PopularUser>> GetPopularUsers(int limit)
        {
            List<UserEntity> users = (await _blogRepository.GetPopularUsers(limit)).ToList();

            List<PopularUser> popUsers = new List<PopularUser>();
            users.ForEach(u =>
            {
                BlogEntity blog = u.Blogs
                .Where(b => { return b.IsPublished != null && b.IsPublished.Value; })
                .OrderBy(b => b.PublishedOn)?
                .FirstOrDefault();
                popUsers.Add(new PopularUser { User = Mapper.Map<UserEntity, User>(u), NumberOfPosts = u.Blogs.Count, PostId = blog?.PostId ?? 0, Title = blog?.Title ?? String.Empty });
            });
            return popUsers;
        }

        public async Task<IEnumerable<BlogPreview>> GetPagedLatestBlogsAsync(int page, int limit)
        {
            List<BlogPreview> posts = (await _blogRepository
                .GetPagedLatestBlogsAsync(page, limit))
                .Select(Mapper.Map<BlogEntity, BlogPreview>)
                .ToList();
            int num = posts.Count();
            return posts;
        }

        public async Task<IEnumerable<T>> GetAllBlogsAsync<T>()
        {
            return (await _blogRepository.GetAllBlogsAsync())
                .Select(Mapper.Map<BlogEntity, T>);
        }
    }
}
