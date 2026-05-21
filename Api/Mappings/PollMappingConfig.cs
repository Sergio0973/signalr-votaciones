using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Dtos.Polls;
using Application.UseCases;
using Domain.Entities;
using Mapster;

namespace Api.Mappings;

public sealed class PollMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Poll, PollDto>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Question, src => src.Question.Value)
            .Map(dest => dest.TotalVotes, src => src.Votes.Count)
            .Map(dest => dest.Options, src => src.Options.Select(o => new PollOptionDto
            {
                Id = o.Id.Value,
                Text = o.Text.Value,
                VoteCount = src.Votes.Count(v => v.OptionId == o.Id)
            }).ToList());

        config.NewConfig<CreatePollRequest, CreatePollCommand>()
            .MapWith(src => new CreatePollCommand(
                src.Question,
                src.Options
            ));

        config.NewConfig<PollOption, PollOptionDto>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Text, src => src.Text.Value)
            .Map(dest => dest.VoteCount, src => 0);

        config.NewConfig<Vote, VoteDto>()
            .Map(dest => dest.VoterId, src => src.VoterId.Value)
            .Map(dest => dest.OptionId, src => src.OptionId.Value)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        config.NewConfig<CreatePollOptionRequest, CreatePollOptionCommand>()
            .MapWith(src => new CreatePollOptionCommand(src.Text));

        config.NewConfig<UpdatePollOptionRequest, UpdatePollOptionCommand>()
            .MapWith(src => new UpdatePollOptionCommand(Guid.Empty, src.Text));

        config.NewConfig<CreateVoteRequest, CreateVoteCommand>()
            .MapWith(src => new CreateVoteCommand(src.VoterId, src.OptionId));
    }
}
