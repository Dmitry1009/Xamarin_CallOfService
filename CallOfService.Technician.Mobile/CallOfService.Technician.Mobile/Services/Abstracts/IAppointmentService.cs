using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CallOfService.Technician.Mobile.Domain;
using Xamarin.Forms;

namespace CallOfService.Technician.Mobile.Services.Abstracts
{
    public interface IAppointmentService
    {
        Task<bool> RetrieveAndSaveAppointments();
        Task<List<Appointment>> AppointmentsByDay(DateTime date);
        Task<Appointment> GetAppointmentByJobId(int jobId);
        Task<Job> GetJobById(int jobId);
        Uri GetFileUri(FileReference fileReference, bool isThumbnil);
		Task<bool> StartJob(int jobId);
		Task<bool> FinishJob(int jobId);
        Task<bool> SubmitNote(int jobNumber, string newNoteText, List<Stream> attachments, DateTime now);
    }
}