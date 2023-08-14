using System.Net.Mail;
using Grpc.Core;
using gRPC.Data;
using gRPC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using HostingEnvironmentExtensions = Microsoft.AspNetCore.Hosting.HostingEnvironmentExtensions;

namespace gRPC.Services;

public class ToDoService : ToDoIt.ToDoItBase
{
    private readonly AppDbContext _context;

    public ToDoService(AppDbContext context)
    {
        _context = context;
    }

    public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext call_context)
    {
        if (request.Title == string.Empty || request.Decsription == string.Empty)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));

        var toDoItem = new ToDoItem
        {
            Title = request.Title,
            Description = request.Decsription
        };

        await _context.ToDoItems.AddAsync(toDoItem);
        await _context.SaveChangesAsync();

        return await Task.FromResult(new CreateToDoResponse
        {
            Id = toDoItem.Id
        });
    }

    public override async Task<ReadtoDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext call_context)
    {
        if (request.Id <= 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Id must be greater than 0!"));

        var toDoItem = await _context.ToDoItems.FirstOrDefaultAsync(x => x.Id == request.Id);

        if (toDoItem != null)
        {
            return await Task.FromResult(new ReadtoDoResponse
            {
                Id = toDoItem.Id,
                Title = toDoItem.Title,
                Description = toDoItem.Description,
                ToDoStatus = toDoItem.ToDoStatus
            });
        }

        throw new RpcException(new Status(StatusCode.NotFound, $"No Task with id {request.Id}"));
    }

    public override async Task<GetallResponse> ListToDo(GetAllRequest request, ServerCallContext call_context)
    {
        var response = new GetallResponse();
        var toDoItems = await _context.ToDoItems.ToListAsync();

        foreach (var item in toDoItems)
        {
            response.ToDo.Add(new ReadtoDoResponse
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                ToDoStatus = item.ToDoStatus
            });
        }

        return await Task.FromResult(response);
    }

    public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext call_context)
    {
        if (request.Id <= 0 || request.Title == string.Empty || request.Description == string.Empty)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));
        }

        var toDoItem = await _context.ToDoItems.FirstOrDefaultAsync(x => x.Id == request.Id);

        if (toDoItem == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"No Task with id {request.Id}"));
        }

        toDoItem.Title = request.Title;
        toDoItem.Description = request.Description;
        toDoItem.ToDoStatus = request.ToDoStatus;

        await _context.SaveChangesAsync();

        return await Task.FromResult(new UpdateToDoResponse
        {
            Id = toDoItem.Id
        });
    }

    public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToDoRequest request, ServerCallContext call_context)
    {
        if (request.Id <= 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Id must be greater than 0!"));

        var toDoItem = await _context.ToDoItems.FirstOrDefaultAsync(x => x.Id == request.Id);

        if (toDoItem == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"No Task with id {request.Id}"));

        _context.ToDoItems.Remove(toDoItem);
        await _context.SaveChangesAsync();

        return await Task.FromResult(new DeleteToDoResponse
        {
            Id = toDoItem.Id
        });
    }

    
}