using System;
using System.Collections.Generic;
using System.Linq;

// Base constraints
public interface IStudent
{
    int StudentId { get; }
    string Name { get; }
    int Semester { get; }
}

public interface ICourse
{
    string CourseCode { get; }
    string Title { get; }
    int MaxCapacity { get; }
    int Credits { get; }
}

// 1. Generic enrollment system
public class EnrollmentSystem<TStudent, TCourse>
    where TStudent : IStudent
    where TCourse : ICourse
{
    private Dictionary<TCourse, List<TStudent>> _enrollments = new();

    // Enroll student with constraints
    public bool EnrollStudent(TStudent student, TCourse course)
    {
        if (student == null || course == null)
            throw new ArgumentNullException();

        if (!_enrollments.ContainsKey(course))
            _enrollments[course] = new List<TStudent>();

        var students = _enrollments[course];

        // Capacity check
        if (students.Count >= course.MaxCapacity)
        {
            Console.WriteLine(" Enrollment failed: Course is full.");
            return false;
        }

        // Already enrolled
        if (students.Any(s => s.StudentId == student.StudentId))
        {
            Console.WriteLine(" Enrollment failed: Student already enrolled.");
            return false;
        }

        // Prerequisite check (for LabCourse)
        if (course is LabCourse lab)
        {
            if (student.Semester < lab.RequiredSemester)
            {
                Console.WriteLine(" Enrollment failed: Prerequisite not met.");
                return false;
            }
        }

        students.Add(student);

        Console.WriteLine($" {student.Name} enrolled in {course.Title}");
        return true;
    }

    // Get students for course
    public IReadOnlyList<TStudent> GetEnrolledStudents(TCourse course)
    {
        if (!_enrollments.ContainsKey(course))
            return new List<TStudent>().AsReadOnly();

        return _enrollments[course].AsReadOnly();
    }

    // Get courses for student
    public IEnumerable<TCourse> GetStudentCourses(TStudent student)
    {
        return _enrollments
            .Where(e => e.Value.Any(s => s.StudentId == student.StudentId))
            .Select(e => e.Key);
    }

    // Calculate student workload
    public int CalculateStudentWorkload(TStudent student)
    {
        return GetStudentCourses(student)
            .Sum(c => c.Credits);
    }

    // Helper for GradeBook
    public bool IsEnrolled(TStudent student, TCourse course)
    {
        return _enrollments.ContainsKey(course) &&
               _enrollments[course].Any(s => s.StudentId == student.StudentId);
    }
}

// 2. Specialized implementations
public class EngineeringStudent : IStudent
{
    public int StudentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Semester { get; set; }

    public string Specialization { get; set; } = string.Empty;
}

public class LabCourse : ICourse
{
    public string CourseCode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int MaxCapacity { get; set; }

    public int Credits { get; set; }

    public string LabEquipment { get; set; } = string.Empty;

    public int RequiredSemester { get; set; }
}

// 3. Generic gradebook
public class GradeBook<TStudent, TCourse>
    where TStudent : IStudent
    where TCourse : ICourse
{
    private Dictionary<(TStudent, TCourse), double> _grades = new();

    private EnrollmentSystem<TStudent, TCourse> _system;

    public GradeBook(EnrollmentSystem<TStudent, TCourse> system)
    {
        _system = system;
    }

    // Add grade with validation
    public void AddGrade(TStudent student, TCourse course, double grade)
    {
        if (grade < 0 || grade > 100)
            throw new ArgumentException("Grade must be between 0 and 100.");

        if (!_system.IsEnrolled(student, course))
            throw new InvalidOperationException("Student not enrolled.");

        _grades[(student, course)] = grade;

        Console.WriteLine($"✅ Grade added: {student.Name} → {grade}");
    }

    // Calculate GPA for student
    public double? CalculateGPA(TStudent student)
    {
        var records = _grades
            .Where(g => g.Key.Item1.StudentId == student.StudentId)
            .ToList();

        if (!records.Any())
            return null;

        double totalPoints = 0;
        int totalCredits = 0;

        foreach (var r in records)
        {
            var course = r.Key.Item2;
            var grade = r.Value;

            totalPoints += grade * course.Credits;
            totalCredits += course.Credits;
        }

        return totalPoints / totalCredits;
    }

    // Find top student in course
    public (TStudent student, double grade)? GetTopStudent(TCourse course)
    {
        var records = _grades
            .Where(g => g.Key.Item2.Equals(course))
            .ToList();

        if (!records.Any())
            return null;

        var top = records
            .OrderByDescending(g => g.Value)
            .First();

        return (top.Key.Item1, top.Value);
    }
}

// 4. TEST SCENARIO
class Program
{
    static void Main()
    {
        var system = new EnrollmentSystem<EngineeringStudent, LabCourse>();
        var gradeBook = new GradeBook<EngineeringStudent, LabCourse>(system);

        // Students
        var s1 = new EngineeringStudent
        {
            StudentId = 1,
            Name = "Rahul",
            Semester = 3,
            Specialization = "AI"
        };

        var s2 = new EngineeringStudent
        {
            StudentId = 2,
            Name = "Neha",
            Semester = 2,
            Specialization = "CS"
        };

        var s3 = new EngineeringStudent
        {
            StudentId = 3,
            Name = "Aman",
            Semester = 5,
            Specialization = "Robotics"
        };

        // Courses
        var c1 = new LabCourse
        {
            CourseCode = "CS101",
            Title = "Programming Lab",
            MaxCapacity = 2,
            Credits = 4,
            RequiredSemester = 2,
            LabEquipment = "PC"
        };

        var c2 = new LabCourse
        {
            CourseCode = "CS301",
            Title = "AI Lab",
            MaxCapacity = 1,
            Credits = 5,
            RequiredSemester = 4,
            LabEquipment = "GPU"
        };

        // Enrollment
        system.EnrollStudent(s1, c1);
        system.EnrollStudent(s2, c1);
        system.EnrollStudent(s3, c1); // fail capacity

        system.EnrollStudent(s2, c2); // fail prerequisite
        system.EnrollStudent(s3, c2);

        // Grades
        gradeBook.AddGrade(s1, c1, 85);
        gradeBook.AddGrade(s2, c1, 90);
        gradeBook.AddGrade(s3, c2, 92);

        // GPA
        Console.WriteLine("\n--- GPA ---");

        Console.WriteLine($"Rahul GPA: {gradeBook.CalculateGPA(s1)}");
        Console.WriteLine($"Neha GPA: {gradeBook.CalculateGPA(s2)}");
        Console.WriteLine($"Aman GPA: {gradeBook.CalculateGPA(s3)}");

        // Top Students
        Console.WriteLine("\n--- Top Students ---");

        var top1 = gradeBook.GetTopStudent(c1);
        var top2 = gradeBook.GetTopStudent(c2);

        Console.WriteLine($"CS101 Topper: {top1?.student.Name} ({top1?.grade})");
        Console.WriteLine($"CS301 Topper: {top2?.student.Name} ({top2?.grade})");

        // Workload
        Console.WriteLine("\n--- Workload ---");

        Console.WriteLine($"Rahul Credits: {system.CalculateStudentWorkload(s1)}");
        Console.WriteLine($"Neha Credits: {system.CalculateStudentWorkload(s2)}");
        Console.WriteLine($"Aman Credits: {system.CalculateStudentWorkload(s3)}");
    }
}
