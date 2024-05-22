#include <iostream>
#include <omp.h>

void task1(int N) {
    int sum = 0;

#pragma omp parallel num_threads(2) reduction(+:sum)
    {
        int partial_sum = 0;
        int thread_num = omp_get_thread_num();

#pragma omp for
        for (int i = 1; i <= N; ++i) {
            if (thread_num == 0 && i <= N / 2) {
                partial_sum += i;
            }
            else if (thread_num == 1 && i > N / 2) {
                partial_sum += i;
            }
        }

#pragma omp critical
        std::cout << "[" << thread_num << "]: Sum = " << partial_sum << std::endl;

        sum += partial_sum;
    }

    std::cout << "Sum = " << sum << std::endl;
}

void task2(int N, int k) {
    int sum = 0;
    omp_set_num_threads(k);

#pragma omp parallel reduction(+:sum)
    {
        int partial_sum = 0;
        int thread_num = omp_get_thread_num();

#pragma omp for
        for (int i = 1; i <= N; ++i) {
            partial_sum += i;
        }

#pragma omp critical
        std::cout << "[" << thread_num << "]: Sum = " << partial_sum << std::endl;

        sum += partial_sum;
    }

    std::cout << "Total Sum = " << sum << std::endl;
}

void task3(int N, int k) {
    int sum = 0;
    omp_set_num_threads(k);

#pragma omp parallel reduction(+:sum)
    {
        int partial_sum = 0;
        int thread_num = omp_get_thread_num();

#pragma omp for
        for (int i = 1; i <= N; ++i) {
            partial_sum += i;
        }

#pragma omp critical
        std::cout << "[" << thread_num << "]: Sum = " << partial_sum << std::endl;

        sum += partial_sum;
    }

    std::cout << "Total Sum = " << sum << std::endl;
}


void task4(int N, int k, const char* schedule_type, int chunk_size = 1) {
    int sum = 0;
    omp_set_num_threads(k);

    if (std::string(schedule_type) == "static") {
#pragma omp parallel for schedule(static, chunk_size) reduction(+:sum)
        for (int i = 1; i <= N; ++i) {
            sum += i;
            int thread_num = omp_get_thread_num();
#pragma omp critical
            std::cout << "[" << thread_num << "]: calculation of the iteration number " << i << std::endl;
        }
    }
    else if (std::string(schedule_type) == "dynamic") {
#pragma omp parallel for schedule(dynamic, chunk_size) reduction(+:sum)
        for (int i = 1; i <= N; ++i) {
            sum += i;
            int thread_num = omp_get_thread_num();
#pragma omp critical
            std::cout << "[" << thread_num << "]: calculation of the iteration number " << i << std::endl;
        }
    }
    else if (std::string(schedule_type) == "guided") {
#pragma omp parallel for schedule(guided, chunk_size) reduction(+:sum)
        for (int i = 1; i <= N; ++i) {
            sum += i;
            int thread_num = omp_get_thread_num();
#pragma omp critical
            std::cout << "[" << thread_num << "]: calculation of the iteration number " << i << std::endl;
        }
    }

    std::cout << "Total Sum = " << sum << std::endl;
}


int main() {
    int N = 10;
    int k = 4;

    std::cout << "Task 1: Sum of numbers from 1 to " << N << " with 2 threads" << std::endl;
    task1(N);
    std::cout << "\n";

    std::cout << "Task 2: Sum of numbers from 1 to " << N << " with " << k << " threads" << std::endl;
    task2(N, k);
    std::cout << "\n";

    std::cout << "Task 3: Parallel sum calculation with " << k << " threads" << std::endl;
    task3(N, k);
    std::cout << "\n";

    std::cout << "Task 4: Static Schedule, Default Chunk Size:" << std::endl;
    task4(N, k, "static");
    std::cout << "\n";

    std::cout << "Static Schedule, Chunk Size 1:" << std::endl;
    task4(N, k, "static", 1);
    std::cout << "\n";

    std::cout << "Static Schedule, Chunk Size 2:" << std::endl;
    task4(N, k, "static", 2);
    std::cout << "\n";

    std::cout << "Dynamic Schedule, Default Chunk Size:" << std::endl;
    task4(N, k, "dynamic");
    std::cout << "\n";

    std::cout << "Dynamic Schedule, Chunk Size 2:" << std::endl;
    task4(N, k, "dynamic", 2);
    std::cout << "\n";

    std::cout << "Guided Schedule, Default Chunk Size:" << std::endl;
    task4(N, k, "guided");
    std::cout << "\n";

    std::cout << "Guided Schedule, Chunk Size 2:" << std::endl;
    task4(N, k, "guided", 2);
    std::cout << "\n";

    return 0;
}
