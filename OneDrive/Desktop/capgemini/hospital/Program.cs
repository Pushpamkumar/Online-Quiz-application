using System;
using System.Collections.Generic;
using System.Linq;

// ---------------- INTERFACES & ENUMS ----------------

public interface IPatient
{
    int PatientId { get; }
    string Name { get; }
    DateTime DateOfBirth { get; }
    BloodType BloodType { get; }
}

public enum BloodType { A, B, AB, O }
public enum Condition { Stable, Critical, Recovering }

// ---------------- 1. PRIORITY QUEUE ----------------

public class PriorityQueue<T> where T : IPatient
{
    private SortedDictionary<int, Queue<T>> _queues = new();

    // Enqueue patient with priority (1=highest, 5=lowest)
    public void Enqueue(T patient, int priority)
    {
        if (patient == null)
            throw new ArgumentNullException();

        if (priority < 1 || priority > 5)
            throw new ArgumentException("Priority must be between 1 and 5.");

        if (!_queues.ContainsKey(priority))
            _queues[priority] = new Queue<T>();

        _queues[priority].Enqueue(patient);

        Console.WriteLine($"{patient.Name} added with priority {priority}");
    }

    // Dequeue highest priority patient
    public T Dequeue()
    {
        foreach (var q in _queues.OrderBy(q => q.Key))
        {
            if (q.Value.Count > 0)
                return q.Value.Dequeue();
        }

        throw new InvalidOperationException("Queue is empty.");
    }

    // Peek without removing
    public T Peek()
    {
        foreach (var q in _queues.OrderBy(q => q.Key))
        {
            if (q.Value.Count > 0)
                return q.Value.Peek();
        }

        throw new InvalidOperationException("Queue is empty.");
    }

    // Get count by priority
    public int GetCountByPriority(int priority)
    {
        if (!_queues.ContainsKey(priority))
            return 0;

        return _queues[priority].Count;
    }
}

// ---------------- 2. MEDICAL RECORD ----------------

public class MedicalRecord<T> where T : IPatient
{
    private T _patient;
    private List<(DateTime date, string diagnosis)> _diagnoses = new();
    private Dictionary<DateTime, string> _treatments = new();

    public MedicalRecord(T patient)
    {
        _patient = patient;
    }

    // Add diagnosis with date
    public void AddDiagnosis(string diagnosis, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(diagnosis))
            throw new ArgumentException("Diagnosis required.");

        _diagnoses.Add((date, diagnosis));
    }

    // Add treatment
    public void AddTreatment(string treatment, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(treatment))
            throw new ArgumentException("Treatment required.");

        _treatments[date] = treatment;
    }

    // Get treatment history
    public IEnumerable<KeyValuePair<DateTime, string>> GetTreatmentHistory()
    {
        return _treatments
            .OrderBy(t => t.Key);
    }
}

// ---------------- 3. PATIENT TYPES ----------------

public class PediatricPatient : IPatient
{
    public int PatientId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public BloodType BloodType { get; set; }

    public string GuardianName { get; set; } = string.Empty;

    public double Weight { get; set; } // kg
}

public class GeriatricPatient : IPatient
{
    public int PatientId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public BloodType BloodType { get; set; }

    public List<string> ChronicConditions { get; } = new();

    public int MobilityScore { get; set; } // 1-10
}

// ---------------- 4. MEDICATION SYSTEM ----------------

public class MedicationSystem<T> where T : IPatient
{
    private Dictionary<T, List<(string medication, DateTime time)>> _medications = new();

    // Prescribe medication with validation
    public void PrescribeMedication(
        T patient,
        string medication,
        Func<T, bool> dosageValidator)
    {
        if (patient == null || string.IsNullOrWhiteSpace(medication))
            throw new ArgumentException();

        if (!dosageValidator(patient))
        {
            Console.WriteLine($"Invalid dosage for {patient.Name}");
            return;
        }

        if (!_medications.ContainsKey(patient))
            _medications[patient] = new List<(string, DateTime)>();

        _medications[patient].Add((medication, DateTime.Now));

        Console.WriteLine($" {medication} prescribed to {patient.Name}");
    }

    // Check drug interactions
    public bool CheckInteractions(T patient, string newMedication)
    {
        if (!_medications.ContainsKey(patient))
            return false;

        // Sample interaction rule
        var dangerousPairs = new Dictionary<string, string>
        {
            { "DrugA", "DrugB" },
            { "Paracetamol", "Ibuprofen" }
        };

        var existing = _medications[patient]
            .Select(m => m.medication);

        foreach (var med in existing)
        {
            if (dangerousPairs.ContainsKey(med) &&
                dangerousPairs[med] == newMedication)
                return true;

            if (dangerousPairs.ContainsKey(newMedication) &&
                dangerousPairs[newMedication] == med)
                return true;
        }

        return false;
    }
}

// ---------------- 5. TEST SCENARIO ----------------

class Program
{
    static void Main()
    {
        Console.WriteLine("Hospital System Started\n");

        // Priority Queue
        var queue = new PriorityQueue<IPatient>();

        // Patients
        var p1 = new PediatricPatient
        {
            PatientId = 1,
            Name = "Amit",
            DateOfBirth = new DateTime(2018, 5, 10),
            BloodType = BloodType.A,
            GuardianName = "Raj",
            Weight = 20
        };

        var p2 = new PediatricPatient
        {
            PatientId = 2,
            Name = "Neha",
            DateOfBirth = new DateTime(2017, 8, 15),
            BloodType = BloodType.B,
            GuardianName = "Sunita",
            Weight = 18
        };

        var g1 = new GeriatricPatient
        {
            PatientId = 3,
            Name = "Ramesh",
            DateOfBirth = new DateTime(1950, 3, 20),
            BloodType = BloodType.O,
            MobilityScore = 5
        };

        var g2 = new GeriatricPatient
        {
            PatientId = 4,
            Name = "Suresh",
            DateOfBirth = new DateTime(1945, 11, 5),
            BloodType = BloodType.AB,
            MobilityScore = 3
        };

        // Add chronic conditions
        g1.ChronicConditions.Add("Diabetes");
        g2.ChronicConditions.Add("Kidney Disease");

        // Enqueue patients
        queue.Enqueue(p1, 3);
        queue.Enqueue(g1, 1);
        queue.Enqueue(p2, 4);
        queue.Enqueue(g2, 2);

        // Medical Records
        var r1 = new MedicalRecord<PediatricPatient>(p1);
        r1.AddDiagnosis("Fever", DateTime.Now.AddDays(-2));
        r1.AddTreatment("Paracetamol", DateTime.Now.AddDays(-1));

        var r2 = new MedicalRecord<GeriatricPatient>(g1);
        r2.AddDiagnosis("Hypertension", DateTime.Now.AddDays(-5));
        r2.AddTreatment("BP Control", DateTime.Now.AddDays(-3));

        // Medication System
        var medSystem = new MedicationSystem<IPatient>();

        // Pediatric validation (weight > 15kg)
        bool PediatricValidator(IPatient p)
        {
            if (p is PediatricPatient child)
                return child.Weight >= 15;

            return true;
        }

        // Geriatric validation (no kidney disease)
        bool GeriatricValidator(IPatient p)
        {
            if (p is GeriatricPatient g)
                return !g.ChronicConditions.Contains("Kidney Disease");

            return true;
        }

        // Prescribe medications
        medSystem.PrescribeMedication(p1, "Paracetamol", PediatricValidator);
        medSystem.PrescribeMedication(p2, "DrugA", PediatricValidator);

        medSystem.PrescribeMedication(g1, "DrugB", GeriatricValidator);
        medSystem.PrescribeMedication(g2, "DrugB", GeriatricValidator); // fail

        // Drug interaction
        Console.WriteLine("\n--- Drug Interaction Check ---");

        bool interaction = medSystem.CheckInteractions(p2, "DrugB");

        Console.WriteLine($"Interaction for Neha: {interaction}");

        // Process patients
        Console.WriteLine("\n--- Patient Processing ---");

        for (int i = 0; i < 4; i++)
        {
            var next = queue.Dequeue();
            Console.WriteLine($"Treating: {next.Name}");
        }

        // Treatment history
        Console.WriteLine("\n--- Treatment History (Amit) ---");

        foreach (var t in r1.GetTreatmentHistory())
        {
            Console.WriteLine($"{t.Key.ToShortDateString()} → {t.Value}");
        }

        Console.WriteLine("\nHospital Workflow Completed");
    }
}
