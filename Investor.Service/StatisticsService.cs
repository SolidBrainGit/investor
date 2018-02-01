﻿using System;
using System.Threading.Tasks;
using AutoMapper;
using Investor.Entity;
using Investor.Model;
using Investor.Repository.Interfaces;
using Investor.Service.Interfaces;
using Investor.Service.Utils;

namespace Investor.Service
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IStatisticsRepository _statisticsRepository;
        private readonly CacheService _cacheService;
        private readonly IMapper _mapper;
        public StatisticsService(IStatisticsRepository statisticsRepository, CacheService cacheService, IMapper mapper)
        {
            _statisticsRepository = statisticsRepository;
            _cacheService = cacheService;
            _mapper = mapper;
        }
        public async Task AddStatisticsAsync(Statistics statistics)
        {
            await _statisticsRepository.AddStatistics(_mapper.Map<Statistics, StatisticsEntity>(statistics));
        }

        public async Task<int> GetPostViewsCountByIdAsync(int postId, bool isUnique = false)
        {
            var key = $"post_{postId}_views_count";

            int? postViewsCount = (int?)_cacheService.GetValue(key);

            if (postViewsCount.HasValue) return postViewsCount.Value;

            postViewsCount = await _statisticsRepository.GetPostViewsCountByIdAsync(postId, isUnique);
            _cacheService.SetValue(key, postViewsCount.Value, TimeSpan.FromMinutes(15));
            return postViewsCount.Value;
        }
    }
}
